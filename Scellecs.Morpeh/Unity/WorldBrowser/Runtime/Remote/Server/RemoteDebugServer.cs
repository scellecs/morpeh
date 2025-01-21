#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.Utils.WorldBrowser.Remote;
using Scellecs.Morpeh.WorldBrowser.Remote.Commands;
using Scellecs.Morpeh.WorldBrowser.Serialization;
using System;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal sealed class RemoteDebugServer : MonoBehaviour {
        private INetworkLogger logger;
        private IModelsStorage modelsStorage;
        private NetworkTransport server;
        private CommandHandlerRegistry commandHandlers;
        private CommandDispatcher dispatcher;
        private bool isInitialized;
        private bool wasConnected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize() {
            if (Application.isEditor) {
                return;
            }

            var go = new GameObject("[RemoteDebugServer]");
            go.AddComponent<RemoteDebugServer>();
            DontDestroyOnLoad(go);
        }

        private void Start() {
            InitializeServer();
        }

        private void Update() {
            if (!this.isInitialized) {
                return;
            }

            var isConnected = this.server.IsConnected;
            if (this.wasConnected && !isConnected) {
                Log("Client disconnected - cleaning up session");
                CleanupSession();
                this.wasConnected = false;
            }
            else if (!this.wasConnected && isConnected) {
                Log("New client connected - initializing session");
                InitializeSession();
                wasConnected = true;
            }

            if (isConnected) {
                try {
                    this.dispatcher.CollectCommands();
                }
                catch (Exception e) {
                    Log($"Error during update: {e}");
                    CleanupSession();
                    this.wasConnected = false;
                }
            }
        }

        private void InitializeServer() {
            if (this.isInitialized) {
                return;
            }
            try {
                this.logger = new ServerLogger();
                this.server = new NetworkTransport(this.logger);
                if (!this.server.StartServer(RemoteWorldBrowserUtils.SERVER_PORT)) {
                    Log("Failed to start server");
                    return;
                }
                this.isInitialized = true;
            }
            catch (Exception e) {
                Log($"Failed to initialize server: {e}");
                Shutdown();
            }
        }

        private void InitializeSession() {
            try {
                InitializeModels();
            }
            catch (Exception e) {
                Log($"Failed to initialize session: {e}");
                CleanupSession();
                wasConnected = false;
            }
        }

        private void InitializeModels() {
            SerializationUtility.AddAdapter(new UnityObjectBuildRuntimeAdapter());
            this.modelsStorage = new ModelsStorage();
            this.modelsStorage.Initialize();
            this.commandHandlers = new CommandHandlerRegistry();
            this.commandHandlers.RegisterHandler(new UpdateModelsReceiver(this.modelsStorage));
            this.commandHandlers.RegisterHandler(new ComponentStorageCommandReceiver(this.modelsStorage.ComponentStorageProcessor));
            this.commandHandlers.RegisterHandler(new HierarchySearchCommandReceiver(this.modelsStorage.HierarchySearchProcessor));
            this.commandHandlers.RegisterHandler(new HierarchyCommandReceiver(this.modelsStorage.HierarchyProcessor));
            this.commandHandlers.RegisterHandler(new InspectorCommandReceiver(this.modelsStorage.InspectorProcessor));
            this.dispatcher = new CommandDispatcher(this.server, this.commandHandlers);
        }

        private void CleanupSession() {
            this.modelsStorage?.Dispose();
            this.modelsStorage = null;
            this.dispatcher = null;
            this.commandHandlers = null;
        }

        private void Shutdown() {
            if (!this.isInitialized) {
                return;
            }
            try {
                CleanupSession();
                this.server?.Dispose();
                this.server = null;
                this.isInitialized = false;
                this.wasConnected = false;
                Log("Debug server stopped");
            }
            catch (Exception e) {
                Log($"Error during shutdown: {e}");
            }
        }

        private void Log(string message) {
            this.logger?.Log(message);
        }

        private void OnDestroy() {
            Shutdown();
        }
    }
}
#endif