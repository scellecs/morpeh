#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Remote.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe sealed class RemoteHierarchySearchCommandSender : IHierarchySearchProcessor, ICommandHandler {
        private readonly HierarchySearchModel model;

        private HierarchySearchSetComponentIncludedCommand? setComponentIncludedCommand;
        private HierarchySearchSetSearchStringCommand? setStringCommand;

        public byte CommandType => CommandTypeId.HierarchySearch;

        internal RemoteHierarchySearchCommandSender() {
            this.model = new HierarchySearchModel();
            this.model.version = -1u;
            this.model.searchString = string.Empty;
            this.model.withSource = new List<int>();
            this.model.withoutSource = new List<int>();
            this.model.includedIds = new HashSet<int>();
            this.model.excludedIds = new HashSet<int>();
            this.setComponentIncludedCommand = null;
            this.setStringCommand = null;
        }

        public void Handle(Command response, NetworkTransport transport) {
            this.HandleFetchResult(response);
            this.SendCommands(transport);
        }

        internal void SendCommands(NetworkTransport transport) {
            var allocator = transport.SendAllocator;

            if (this.setStringCommand != null) {
                transport.PushSend(this.setStringCommand.Value.Serialize(allocator, out var length), length);
                this.setStringCommand = null;
            }

            if (this.setComponentIncludedCommand != null) {
                transport.PushSend(this.setComponentIncludedCommand.Value.Serialize(allocator, out var length), length);
                this.setComponentIncludedCommand = null;
            }

            var fetchCommand = new HierarchySearchFetchCommand() { version = this.model.version };
            transport.PushSend(fetchCommand.Serialize(allocator, out var len), len);
        }

        private void HandleFetchResult(Command response) {
            if (response.CommandId != HierarchySearchCommand.FetchResponse) {
                Debug.LogError("Invalid command id");
                return;
            }

            var fetchResult = response.Deserialize<HierarchySearchFetchResponse>();

            if (fetchResult.hierarchySearch.version != this.model.version) {
                this.model.FromSerializable(ref fetchResult.hierarchySearch);
            }
        }

        public HierarchySearchModel GetModel() { 
            return this.model;
        }

        public void SetSearchString(string input) {
            this.setStringCommand = new HierarchySearchSetSearchStringCommand() { value = input };
        }

        public void SetComponentIncluded(int id, bool included, QueryParam param) {
            this.setComponentIncludedCommand = new HierarchySearchSetComponentIncludedCommand() {
                id = id,
                included = included,
                param = param
            };
        }
    }
}
#endif
