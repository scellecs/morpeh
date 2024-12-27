#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.IO;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe sealed class NetworkTransport : IDisposable {
        private readonly INetworkLogger logger;
        private NetworkConnection connection;
        private TcpListener listener;
        private Thread listenerThread;
        private Thread receiveThread;
        private Thread sendThread;
        private volatile bool isListening;
        private volatile bool isProcessing;

        internal bool IsConnected => this.connection?.Connected ?? false;
        internal NetworkAllocator SendAllocator => this.connection?.sendPipe.Allocator;

        internal NetworkTransport(INetworkLogger logger = null) {
            this.logger = logger;
        }

        public void Log(string message) {
            this.logger?.Log(message);
        }

        internal bool Connect(string ip, int port) {
            try {
                this.Log($"Connecting to {ip}:{port}...");
                var client = new TcpClient();
                this.ConfigureClient(client);
                client.Connect(ip, port);
                this.connection = new NetworkConnection(client);
                this.StartThreads();
                this.Log($"Successfully connected to {ip}:{port}");
                return true;
            }
            catch (Exception e) {
                this.Log($"Failed to connect to {ip}:{port}: {e}");
                return false;
            }
        }

        internal bool StartServer(int port) {
            try {
                this.Log($"Starting server on port {port}...");
                this.isListening = true;
                this.listener = new TcpListener(IPAddress.Any, port);
                this.listener.Start();
                this.listenerThread = new Thread(this.ListenForClient) {
                    IsBackground = true,
                    Name = "Transport Listen Thread"
                };
                this.listenerThread.Start();
                this.Log($"Server started on port {port}");
                return true;
            }
            catch (Exception e) {
                this.Log($"Failed to start server on port {port}: {e}");
                return false;
            }
        }

        internal void Stop() {
            this.isListening = false;
            this.isProcessing = false;
            this.listener?.Stop();
            this.listenerThread?.Join(1000);
            this.CleanupConnection();
            this.Log("Stopped");
        }

        internal bool TryDequeue(out byte* data, out int length) {
            if (this.connection?.receivePipe.TryDequeue(out var message) == true) {
                data = message.data;
                length = message.length;
                return true;
            }

            data = null;
            length = 0;
            return false;
        }

        internal void PushSend(byte* data, int length) {
            this.connection?.sendPipe.Push(data, length);
        }

        internal void EndSend() {
            this.connection?.sendPipe.Allocator.Update();
        }

        internal void EndReceive() {
            this.connection?.receivePipe.Allocator.Update();
        }

        private void ConfigureClient(TcpClient client) {
            client.NoDelay = true;
            client.SendTimeout = 0;
            client.ReceiveTimeout = 0;
            client.Client.Blocking = true;
        }

        private void ListenForClient() {
            try {
                this.Log("Starting to listen for clients...");
                while (this.isListening) {
                    this.Log("Waiting for client connection...");
                    var client = this.listener.AcceptTcpClient();
                    this.ConfigureClient(client);

                    lock (this) {
                        if (this.connection != null) {
                            this.Log("Rejecting new connection - already have an active connection");
                            client.Close();
                            continue;
                        }

                        var endpoint = client.Client.RemoteEndPoint;
                        this.Log($"Client connected from {endpoint}");
                        this.connection = new NetworkConnection(client);
                        this.StartThreads();
                    }
                }
            }
            catch (SocketException e) {
                this.Log($"Listen socket closed: {e.Message}");
            }
            catch (Exception e) {
                this.Log($"Error in listener thread: {e}");
                this.Stop();
            }
            finally {
                this.listener?.Stop();
            }
        }

        private void StartThreads() {
            this.isProcessing = true;

            this.receiveThread = new Thread(this.ReceiveLoop) {
                IsBackground = true,
                Name = "Transport Receive Thread"
            };
            this.receiveThread.Start();

            this.sendThread = new Thread(this.SendLoop) {
                IsBackground = true,
                Name = "Transport Send Thread"
            };
            this.sendThread.Start();
        }

        private void ReceiveLoop() {
            var headerBuffer = stackalloc byte[4];
            try {
                while (this.isProcessing && this.IsConnected) {
                    try {
                        if (!this.ReadExactly(this.connection.stream, new Span<byte>(headerBuffer, 4))) {
                            this.Log("Failed to read message header");
                            break;
                        }

                        var length = *(int*)headerBuffer;
                        if (length <= 0) {
                            this.Log($"Invalid message length: {length}");
                            break;
                        }

                        var data = this.connection.receivePipe.Allocator.Alloc(length);
                        if (!this.ReadExactly(this.connection.stream, new Span<byte>(data, length))) {
                            this.Log("Failed to read message body");
                            break;
                        }

                        this.connection.receivePipe.Push(data, length);
                    }
                    catch (Exception e) {
                        this.Log($"Error in receive loop: {e}");
                        break;
                    }
                }
            }
            finally {
                this.CleanupConnection();
                this.Log("Receive loop ended");
            }
        }

        private void SendLoop() {
            var headerBuffer = stackalloc byte[4];
            while (this.isProcessing && this.IsConnected) {
                this.connection.sendPipe.WaitHandle.WaitOne();
                if (!this.isProcessing) {
                    this.Log("Send loop stopping - transport no longer running");
                    break;
                }

                try {
                    while (this.connection?.sendPipe.TryDequeue(out var message) == true) {
                        *(int*)headerBuffer = message.length;
                        this.connection.stream.Write(new ReadOnlySpan<byte>(headerBuffer, 4));
                        this.connection.stream.Write(new ReadOnlySpan<byte>(message.data, message.length));
                    }
                }
                catch (Exception e) {
                    this.Log($"Error in send loop: {e}");
                    break;
                }

                this.connection?.sendPipe.WaitHandle.Reset();
            }
            this.Log("Send loop ended");
        }

        private bool ReadExactly(NetworkStream stream, Span<byte> buffer) {
            var read = 0;
            while (read < buffer.Length) {
                try {
                    var count = stream.Read(buffer[read..]);
                    if (count <= 0) {
                        this.Log($"Read returned {count}");
                        return false;
                    }
                    read += count;
                }
                catch (IOException e) when (e.InnerException is SocketException { ErrorCode: 10035 }) {
                    Thread.Sleep(1);
                    continue;
                }
                catch (Exception e) {
                    this.Log($"Error reading from stream: {e}");
                    return false;
                }
            }
            return true;
        }

        private void CleanupConnection() {
            lock (this) {
                if (this.connection != null) {
                    this.connection.Dispose();
                    this.connection = null;
                }
            }
        }

        public void Dispose() {
            this.Stop();
        }
    }
}
#endif