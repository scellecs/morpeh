#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class HierarchySearchCommandReceiver : ICommandHandler {
        private readonly HierarchySearchModel model;
        private readonly IHierarchySearchProcessor processor;

        public byte CommandType => CommandTypeId.HierarchySearch;

        internal HierarchySearchCommandReceiver(IHierarchySearchProcessor processor) {
            this.model = processor.GetModel();
            this.processor = processor;
        }

        public void Handle(Command command, NetworkTransport transport) {
            switch (command.CommandId) {
                case HierarchySearchCommand.FetchRequest:
                    HandleFetch(command, transport);
                    break;

                case HierarchySearchCommand.SetComponentIncluded:
                    HandleSetComponentIncluded(command);
                    break;

                case HierarchySearchCommand.SetSearchString:
                    HandleSetSearchString(command);
                    break;
            }
        }

        private void HandleSetComponentIncluded(Command command) {
            var commandData = command.Deserialize<HierarchySearchSetComponentIncludedCommand>();
            this.processor.SetComponentIncluded(commandData.id, commandData.included, commandData.param);
        }

        private void HandleSetSearchString(Command command) {
            var commandData = command.Deserialize<HierarchySearchSetSearchStringCommand>();
            this.processor.SetSearchString(commandData.value);
        }

        private void HandleFetch(Command command, NetworkTransport transport) {
            var commandData = command.Deserialize<HierarchySearchFetchCommand>();
            var allocator = transport.SendAllocator;
            var fetchResult = default(HierarchySearchFetchResponse);
            fetchResult.hierarchySearch.version = this.model.version;

            if (commandData.version != this.model.version) {
                fetchResult.hierarchySearch = this.model.ToSerializable();
            } 

            transport.PushSend(fetchResult.Serialize(allocator, out var length), length);
        }
    }
}
#endif
