#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class InspectorCommandReceiver : ICommandHandler {
        private readonly InspectorModel model;
        private readonly IInspectorProcessor processor;

        public byte CommandType => CommandTypeId.Inspector;

        internal InspectorCommandReceiver(IInspectorProcessor processor) {
            this.model = processor.GetModel();
            this.processor = processor;
        }

        public void Handle(Command command, NetworkTransport transport) {
            switch (command.CommandId) {
                case InspectorCommand.FetchRequest:
                    HandleFetch(transport);
                    break;

                case InspectorCommand.AddComponent:
                    HandleAddComponent(command);
                    break;

                case InspectorCommand.RemoveComponent:
                    HandleRemoveComponent(command);
                    break;

                case InspectorCommand.SetComponent:
                    HandleSetComponent(command);
                    break;

                case InspectorCommand.SetAddComponentSearchString:
                    HandleSetAddComponentSearchString(command);
                    break;

                default:
                    transport.Log($"[InspectorCommandReceiver]: Invalid command id {command.CommandId}");
                    break;
            }
        }

        private void HandleAddComponent(Command command) {
            var commandData = command.Deserialize<InspectorAddComponentCommand>();
            this.processor.AddComponentData(commandData.id);
        }

        private void HandleRemoveComponent(Command command) { 
            var commandData = command.Deserialize<InspectorRemoveComponentCommand>();
            this.processor.RemoveComponentData(commandData.typeId);
        }

        private void HandleSetComponent(Command command) { 
            var commandData = command.Deserialize<InspectorSetComponentCommand>();
            this.processor.SetComponentData(commandData.componentData);
        }

        private void HandleSetAddComponentSearchString(Command command) {
            var commandData = command.Deserialize<InspectorSetAddComponentSearchStringCommand>();
            this.processor.SetAddComponentSearchString(commandData.value);
        }

        private void HandleFetch(NetworkTransport transport) {
            var allocator = transport.SendAllocator;
            var fetchResult = new InspectorFetchResponse() { inspector = this.model.ToSerializable() };
            transport.PushSend(fetchResult.Serialize(allocator, out var length), length);
        }
    }
}
#endif
