#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class InspectorCommandReceiver : ICommandHandler {
        private readonly InspectorModel model;

        public byte CommandType => CommandTypeId.Inspector;

        internal InspectorCommandReceiver(IInspectorProcessor processor) {
            this.model = processor.GetModel();
        }

        public void Handle(Command command, NetworkTransport transport) {
            if (command.CommandId != InspectorCommand.FetchRequest) {
                Debug.LogError("Invalid command id");
                return;
            }

            var allocator = transport.SendAllocator;
            var fetchResult = new InspectorFetchResponse() { inspector = this.model.ToSerializable() };
            transport.PushSend(fetchResult.Serialize(allocator, out var length), length);
        }
    }
}
#endif
