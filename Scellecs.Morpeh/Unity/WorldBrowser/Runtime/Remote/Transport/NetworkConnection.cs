#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Net.Sockets;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal sealed class NetworkConnection {
        internal readonly TcpClient client;
        internal readonly NetworkStream stream;
        internal readonly MessagePipe sendPipe;
        internal readonly MessagePipe receivePipe;

        internal bool Connected => client != null && client.Client != null && client.Client.Connected;

        internal NetworkConnection(TcpClient client) {
            this.client = client;
            this.stream = client.GetStream();
            this.sendPipe = new MessagePipe();
            this.receivePipe = new MessagePipe();
        }

        internal void Dispose() {
            sendPipe.WaitHandle.Set();
            receivePipe.WaitHandle.Set();
            stream.Close();
            client.Close();
            sendPipe.Allocator.Dispose();
            receivePipe.Allocator.Dispose();
        }
    }
}
#endif
