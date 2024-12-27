#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class ComponentStorageCommandReceiver : ICommandHandler {
        private readonly ComponentStorageModel model;

        public byte CommandType => CommandTypeId.ComponentStorage;

        internal ComponentStorageCommandReceiver(IComponentStorageProcessor storage) {
            this.model = storage.GetModel();
        }

        public void Handle(Command command, NetworkTransport transport) {
            if (command.CommandId != ComponentStorageCommand.FetchRequest) {
                Debug.LogError("Invalid command id");
                return;
            }

            var allocator = transport.SendAllocator;
            var commandData = command.Deserialize<ComponentStorageFetchCommand>();
            var fetchResult = default(ComponentStorageFetchResponse);
            fetchResult.storage.version = this.model.version;

            if (commandData.version != this.model.version) {
                fetchResult.storage = this.model.ToSerializable();
            }

            transport.PushSend(fetchResult.Serialize(allocator, out var length), length);
        }
    }
}
#endif
