#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class RemoteInspectorCommandSender : IInspectorProcessor, ICommandHandler, IDisposable {
        private readonly InspectorModel model;
        private readonly UnityObjectEditorAdapter unityObjectAdapter;

        private InspectorAddComponentCommand? addComponentCommand;
        private InspectorRemoveComponentCommand? removeComponentCommand;
        private InspectorSetComponentCommand? setComponentCommand;
        private InspectorSetAddComponentSearchStringCommand? setComponentSearchStringCommand;

        public byte CommandType => CommandTypeId.Inspector;

        internal RemoteInspectorCommandSender() {
            this.model = new InspectorModel();
            this.model.version = -1u;
            this.model.components = new List<ComponentDataBoxed>();
            this.model.addComponentSuggestions = new List<int>();
            this.model.selectedEntity = default;
            this.unityObjectAdapter = SerializationUtility.GetAdapter<UnityObjectEditorAdapter>();
        }

        public void Handle(Command response, NetworkTransport transport) {
            this.HandleFetchResult(response);
            this.SendCommands(transport);
        }

        internal void SendCommands(NetworkTransport transport) {
            var allocator = transport.SendAllocator;

            if (this.addComponentCommand != null) {
                transport.PushSend(this.addComponentCommand.Value.Serialize(allocator, out var length), length);
                this.addComponentCommand = null;
            }

            if (this.removeComponentCommand != null) {
                transport.PushSend(this.removeComponentCommand.Value.Serialize(allocator, out var length), length);
                this.removeComponentCommand = null;
            }

            if (this.setComponentCommand != null) {
                transport.PushSend(this.setComponentCommand.Value.Serialize(allocator, out var length), length);
                this.setComponentCommand = null;
            }

            if(this.setComponentSearchStringCommand != null) {
                transport.PushSend(this.setComponentSearchStringCommand.Value.Serialize(allocator, out var length), length);
                this.setComponentSearchStringCommand = null;
            }

            var fetchCommand = new InspectorFetchCommand() { version = this.model.version };
            transport.PushSend(fetchCommand.Serialize(allocator, out var len), len);
        }

        private void HandleFetchResult(Command response) {
            if (response.CommandId != InspectorCommand.FetchResponse) {
                Debug.LogError("Invalid command id");
                return;
            }

            this.unityObjectAdapter.Refresh();
            var fetchResult = response.Deserialize<InspectorFetchResponse>();
            this.model.FromSerializable(ref fetchResult.inspector);
        }

        public InspectorModel GetModel() { 
            return this.model; 
        }

        public void AddComponentData(int id) {
            this.addComponentCommand = new InspectorAddComponentCommand() { id = id };
        }

        public void RemoveComponentData(int typeId) {
            this.removeComponentCommand = new InspectorRemoveComponentCommand() { typeId = typeId };
        }

        public void SetComponentData(ComponentDataBoxed componentData) {
            this.setComponentCommand = new InspectorSetComponentCommand() { componentData = componentData };
        }

        public void SetAddComponentSearchString(string value) {
            this.setComponentSearchStringCommand = new InspectorSetAddComponentSearchStringCommand() { value = value };
        }

        public void Dispose() {
            this.unityObjectAdapter?.Cleanup();
        }
    }
}
#endif
