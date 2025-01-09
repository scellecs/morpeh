#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class RemoteInspectorCommandSender : IInspectorProcessor, ICommandHandler, IDisposable {
        private readonly InspectorModel model;
        private readonly UnityObjectAdapter unityObjectAdapter;

        public byte CommandType => CommandTypeId.Inspector;

        internal RemoteInspectorCommandSender() {
            this.model = new InspectorModel();
            this.model.version = -1u;
            this.model.components = new List<ComponentDataBoxed>();
            this.model.selectedEntity = default;
            this.unityObjectAdapter = SerializationUtility.GetAdapter<UnityObjectAdapter>();
        }

        public void Handle(Command response, NetworkTransport transport) {
            this.HandleFetchResult(response);
            this.SendCommands(transport);
        }

        internal void SendCommands(NetworkTransport transport) {
            var allocator = transport.SendAllocator;
            var fetchCommand = new InspectorFetchCommand() { version = this.model.version };
            transport.PushSend(fetchCommand.Serialize(allocator, out var length), length);
        }

        private void HandleFetchResult(Command response) {
            if (response.CommandId != InspectorCommand.FetchResponse) {
                Debug.LogError("Invalid command id");
                return;
            }

            this.unityObjectAdapter.Cleanup();
            var fetchResult = response.Deserialize<InspectorFetchResponse>();
            this.model.FromSerializable(ref fetchResult.inspector);
        }

        public InspectorModel GetModel() { 
            return this.model; 
        }

        public void AddComponentData(int id) {
            // temporarily discard for remote debug
        }

        public void RemoveComponentData(int id) {
            // temporarily discard for remote debug
        }

        public void SetComponentData(int typeId, object data) {
            // temporarily discard for remote debug
        }

        public void SetAddComponentSearchString(string value) {
            // temporarily discard for remote debug
        }

        public void Dispose() {
            this.unityObjectAdapter?.Cleanup();
        }
    }
}
#endif
