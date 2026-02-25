#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.Utils.WorldBrowser.Remote;
using Scellecs.Morpeh.WorldBrowser.Remote.Commands;
using Scellecs.Morpeh.WorldBrowser.Serialization;
using System;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal sealed class RemoteModelsStorage : IModelsStorage {
        private NetworkTransport client;
        private CommandHandlerRegistry commandHandlers;
        private CommandDispatcher dispatcher;
        private Action<bool> OnStateChanged;

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
            this.OnStateChanged = onStateChanged;
        }

        public bool Initialize() {
            try {
                var ipString = UnityEditor.EditorPrefs.GetString(RemoteWorldBrowserUtils.EDITOR_PREFS_IP_KEY);
                if (!RemoteWorldBrowserUtils.ParseIP(ipString, out var ip, out var port)) {
                    Debug.LogError("Failed to parse ip address");
                    return false;
                }

                this.client = new NetworkTransport(new ServerLogger());

                if (!this.client.Connect(ip, port)) {
                    this.OnStateChanged(false);
                    return false;
                }

                SerializationUtility.AddAdapter(new UnityObjectEditorAdapter());
                this.commandHandlers = new CommandHandlerRegistry();
                this.commandHandlers.RegisterHandler(this.modelsHandler = new RemoteUpdateModelsSender());
                this.commandHandlers.RegisterHandler(this.componentStorage = new RemoteComponentStorageCommandSender());
                this.commandHandlers.RegisterHandler(this.hierarchySearch = new RemoteHierarchySearchCommandSender());
                this.commandHandlers.RegisterHandler(this.hierarchy = new RemoteHierarchyCommandSender());
                this.commandHandlers.RegisterHandler(this.inspector = new RemoteInspectorCommandSender());
                this.dispatcher = new CommandDispatcher(this.client, this.commandHandlers);
                this.firstUpdate = true;
            }
            catch (Exception e) {
                Debug.LogException(e);
                this.OnStateChanged(false);
                return false;
            }

            return true;
        }

        public void Update() {
            if (!this.client.IsConnected) {
                this.OnStateChanged(false);
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
                this.dispatcher.CollectCommands();
            }
            catch (Exception e) {
                Debug.LogError($"Error during update: {e}");
                this.client.Stop();
                this.OnStateChanged(false);
            }
        }

        public void Dispose() {
            this.client?.Dispose();
            this.inspector?.Dispose();
        }
    }
}
#endif