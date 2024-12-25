#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Scellecs.Morpeh.Utils.WorldBrowser.Remote;
using Scellecs.Morpeh.WorldBrowser.Remote.Commands;
using System;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal sealed class RemoteModelsStorage : IModelsStorage {
        private NetworkTransport client;
        private CommandHandlerRegistry commandHandlers;
        private CommandDispatcher dispatcher;
        private Action<bool> onStateChanged;

        private RemoteUpdateModelsSender modelsHandler;
        private RemoteComponentStorageCommandSender componentStorage;
        private RemoteHierarchySearchCommandSender hierarchySearch;
        private RemoteHierarchyCommandSender hierarchy;
        private RemoteInspectorCommandSender inspector;

        public IComponentStorageProcessor ComponentStorageProcessor => this.componentStorage;
        public IHierarchySearchProcessor HierarchySearchProcessor => this.hierarchySearch;
        public IHierarchyProcessor HierarchyProcessor => this.hierarchy;
        public IInspectorProcessor InspectorProcessor => this.inspector;

        private bool firstUpdate;

        internal RemoteModelsStorage(Action<bool> onStateChanged) {
            this.onStateChanged = onStateChanged;
        }

        public bool Initialize() {
            try {
                this.client = new NetworkTransport(new ServerLogger());
                if (!this.client.Connect("127.0.0.1", 9999)) {
                    this.onStateChanged(false);
                    return false;
                }

                this.commandHandlers = new CommandHandlerRegistry();
                this.commandHandlers.RegisterHandler(this.modelsHandler = new RemoteUpdateModelsSender());
                this.commandHandlers.RegisterHandler(this.componentStorage = new RemoteComponentStorageCommandSender());
                this.commandHandlers.RegisterHandler(this.hierarchySearch = new RemoteHierarchySearchCommandSender());
                this.commandHandlers.RegisterHandler(this.hierarchy = new RemoteHierarchyCommandSender());
                this.commandHandlers.RegisterHandler(this.inspector = new RemoteInspectorCommandSender());
                this.dispatcher = new CommandDispatcher(client, commandHandlers);
                this.firstUpdate = true;
            }
            catch (Exception e) {
                Debug.LogException(e);
                this.onStateChanged(false);
                return false;
            }

            return true;
        }

        public void Update() {
            if (!client.IsConnected) {
                this.onStateChanged(false);
                return;
            }

            if (this.firstUpdate) {
                this.dispatcher.BeginBatch();
                this.modelsHandler.SendCommands(this.client);
                this.componentStorage.SendCommands(this.client);
                this.hierarchySearch.SendCommands(this.client);
                this.hierarchy.SendCommands(this.client);
                this.inspector.SendCommands(this.client);
                this.dispatcher.EndBatch();
                this.firstUpdate = false;
                return;
            }

            try {
                dispatcher.CollectCommands();
            }
            catch (Exception e) {
                Debug.LogError($"Error during update: {e}");
                this.client.Stop();
                this.onStateChanged(false);
            }
        }

        public void Dispose() {
            this.client?.Dispose();
            this.inspector?.Dispose();
        }
    }
}
#endif