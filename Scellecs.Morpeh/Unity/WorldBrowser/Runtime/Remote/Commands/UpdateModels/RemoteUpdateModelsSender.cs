#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class RemoteUpdateModelsSender : ICommandHandler {
        public byte CommandType => CommandTypeId.Models;

        public void Handle(Command command, NetworkTransport transport) {
            if (command.CommandId != ModelsCommand.UpdateResponse) {
                Debug.LogError("Invalid command id");
                return;
            }

            this.SendCommands(transport);
        }

        internal void SendCommands(NetworkTransport transport) {
            transport.PushSend(default(UpdateModelsCommand).Serialize(transport.SendAllocator, out var length), length);
        }
    }
}
#endif
