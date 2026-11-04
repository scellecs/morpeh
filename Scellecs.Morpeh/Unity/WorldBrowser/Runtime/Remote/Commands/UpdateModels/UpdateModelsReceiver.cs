#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class UpdateModelsReceiver : ICommandHandler {
        private readonly IModelsStorage modelsStorage;

        public byte CommandType => CommandTypeId.Models;

        internal UpdateModelsReceiver(IModelsStorage modelsStorage) {
            this.modelsStorage = modelsStorage;
        }

        public void Handle(Command command, NetworkTransport transport) {
            if (command.CommandId != ModelsCommand.UpdateRequest) {
                Debug.LogError("Invalid command id");
                return;
            }

            this.modelsStorage.Update();
            transport.PushSend(default(UpdateModelsResponse).Serialize(transport.SendAllocator, out var length), length);
        }
    }
}
#endif
