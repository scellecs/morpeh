#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class HierarchyCommandReceiver : ICommandHandler {
        private readonly HierarchyModel model;
        private readonly IHierarchyProcessor processor;

        public byte CommandType => CommandTypeId.Hierarchy;

        internal HierarchyCommandReceiver(IHierarchyProcessor processor) {
            this.model = processor.GetModel();
            this.processor = processor;
        }

        public void Handle(Command command, NetworkTransport transport) {
            switch (command.CommandId) {
                case HierarchyCommand.FetchRequest:
                    HandleFetchRequest(command, transport);
                    break;

                case HierarchyCommand.SetSelectedWorldId:
                    HandleSetSelectedWorld(command);
                    break;

                case HierarchyCommand.SetSelectedEntity:
                    HandleSetSelectedEntity(command);
                    break;
            }
        }

        private void HandleSetSelectedWorld(Command command) { 
            var commandData = command.Deserialize<HierarchySetSelectedWorldIdCommand>();
            this.processor.SetSelectedWorldId(commandData.worldId, commandData.state);
        }

        private void HandleSetSelectedEntity(Command command) {
            var commandData = command.Deserialize<HierarchySetSelectedEntityCommand>();
            this.processor.SetSelectedEntity(commandData.entity);
        }

        private void HandleFetchRequest(Command command, NetworkTransport transport) {
            var commandData = command.Deserialize<HierarchyFetchCommand>();
            var allocator = transport.SendAllocator;
            var fetchResult = default(HierarchyFetchResponse);
            fetchResult.hierarchy.version = this.model.version;

            if (commandData.version != this.model.version) {
                fetchResult.hierarchy = this.model.ToSerializable();
            }

            transport.PushSend(fetchResult.Serialize(allocator, out var length), length);
        }
    }
}
#endif
