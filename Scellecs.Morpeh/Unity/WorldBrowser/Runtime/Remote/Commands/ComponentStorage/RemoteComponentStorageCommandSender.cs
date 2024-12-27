#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class RemoteComponentStorageCommandSender : IComponentStorageProcessor, ICommandHandler {
        private readonly ComponentStorageModel model;

        public byte CommandType => CommandTypeId.ComponentStorage;

        internal RemoteComponentStorageCommandSender() {
            this.model = new ComponentStorageModel();
            this.model.version = -1u;
            this.model.componentNames = new List<string>();
            this.model.typeIdToInternalId = new Dictionary<int, int>();
        }

        public void Handle(Command response, NetworkTransport transport) {
            this.HandleFetchResult(response);
            this.SendCommands(transport);
        }

        internal void SendCommands(NetworkTransport transport) {
            var allocator = transport.SendAllocator;
            var fetchCommand = new ComponentStorageFetchCommand() { version = this.model.version };
            transport.PushSend(fetchCommand.Serialize(allocator, out var length), length);
        }

        private void HandleFetchResult(Command response) {
            if (response.CommandId != ComponentStorageCommand.FetchResponse) {
                Debug.LogError("Invalid command id");
                return;
            }

            var fetchResult = response.Deserialize<ComponentStorageFetchResponse>();

            if (fetchResult.storage.version != this.model.version) {
                this.model.FromSerializable(ref fetchResult.storage);
            }
        }

        public ComponentStorageModel GetModel() {
            return this.model;
        }
    }
}
#endif
