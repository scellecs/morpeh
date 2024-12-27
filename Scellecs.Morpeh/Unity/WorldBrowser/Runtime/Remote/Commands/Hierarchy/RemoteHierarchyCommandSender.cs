#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe sealed class RemoteHierarchyCommandSender : IHierarchyProcessor, ICommandHandler {
        private readonly HierarchyModel model;

        private HierarchySetSelectedWorldIdCommand? setSelectedWorldCommand;
        private HierarchySetSelectedEntityCommand? setSelectedEntityCommand;

        public byte CommandType => CommandTypeId.Hierarchy;

        internal RemoteHierarchyCommandSender() {
            this.model = new HierarchyModel();
            this.model.version = -1u;
            this.model.worldIds = new List<int>();
            this.model.selectedWorldIds = new HashSet<int>();
            this.model.entities = new List<Entity>();
            this.setSelectedWorldCommand = null;
            this.setSelectedEntityCommand = null;
        }

        public void Handle(Command response, NetworkTransport transport) {
            this.HandleFetchResult(response);
            this.SendCommands(transport);
        }

        internal void SendCommands(NetworkTransport transport) {
            var allocator = transport.SendAllocator;

            if (this.setSelectedWorldCommand != null) {
                transport.PushSend(this.setSelectedWorldCommand.Value.Serialize(allocator, out var length), length);
                this.setSelectedWorldCommand = null;
            }

            if (this.setSelectedEntityCommand != null) {
                transport.PushSend(this.setSelectedEntityCommand.Value.Serialize(allocator, out var length), length);
                this.setSelectedEntityCommand = null;
            }

            var fetchCommand = new HierarchyFetchCommand() { version = this.model.version };
            transport.PushSend(fetchCommand.Serialize(allocator, out var len), len);
        }

        private void HandleFetchResult(Command response) {
            if (response.CommandId != HierarchyCommand.FetchResponse) {
                Debug.LogError("Invalid command id");
                return;
            }

            var fetchResult = response.Deserialize<HierarchyFetchResponse>();

            if (fetchResult.hierarchy.version != this.model.version) {
                this.model.FromSerializable(ref fetchResult.hierarchy);
            }
        }

        public HierarchyModel GetModel() { 
            return this.model; 
        }

        public void SetSelectedEntity(Entity entity) {
            this.setSelectedEntityCommand = new HierarchySetSelectedEntityCommand() { entity = entity };
        }

        public void SetSelectedWorldId(int id, bool state) {
            this.setSelectedWorldCommand = new HierarchySetSelectedWorldIdCommand() {
                worldId = id,
                state = state,
            };
        }
    }
}
#endif
