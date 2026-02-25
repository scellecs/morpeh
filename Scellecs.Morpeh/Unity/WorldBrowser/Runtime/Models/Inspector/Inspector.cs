#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Filter;
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class Inspector : IInspectorProcessor {
        private readonly InspectorModel model;
        private readonly Hierarchy hierarchy;
        private readonly ComponentStorage storage;

        private List<int> addComponentSuggestions;
        private string addComponentSearchString;

        private EntityHandle currentHandle;
        private long storageVersion;

        internal Inspector(Hierarchy hierarchy, ComponentStorage storage) {
            this.model = new InspectorModel();
            this.hierarchy = hierarchy;
            this.storage = storage;
            this.model.components = new List<ComponentDataBoxed>();
            this.model.addComponentSuggestions = this.addComponentSuggestions = new List<int>();
            this.addComponentSearchString = string.Empty;
            this.currentHandle = default;
            this.storageVersion = -1u;
        }

        internal void Update() {
            var handle = this.hierarchy.GetSelectedEntityHandle();
            var requireUpdateSuggestions = false;

            if (!this.currentHandle.IsValid && !handle.IsValid) {
                if (this.model.components.Count > 0) {
                    this.currentHandle = default;
                    this.model.components.Clear();
                    this.model.addComponentSuggestions.Clear();
                    this.model.selectedEntity = default;
                    this.model.IncrementVersion();
                }

                return;
            }


            if (!this.currentHandle.Equals(handle)) {
                this.currentHandle = handle;
                this.model.selectedEntity = this.currentHandle.entity;
                this.model.IncrementVersion();
                requireUpdateSuggestions = true;
            }

            this.storage.FetchEntityComponents(this.currentHandle.entity, this.model.components);

            if (this.storageVersion != this.storage.GetVersion()) {
                this.storageVersion = this.storage.GetVersion();
                this.model.IncrementVersion();
                requireUpdateSuggestions = true;
            }

            if (requireUpdateSuggestions) {
                this.UpdateAddComponentSuggestions();
            }
        }

        public InspectorModel GetModel() {
            return this.model;
        }

        public void AddComponentData(int id) { 
            var typeId = this.storage.GetTypeIdByComponentId(id);
            this.storage.AddComponentData(typeId, this.currentHandle.entity);
        }

        public void RemoveComponentData(int typeId) {
            this.storage.RemoveComponentData(typeId, this.currentHandle.entity);
        }

        public void SetComponentData(ComponentDataBoxed componentData) {
            if (!componentData.isNotSerialized) {
                this.storage.SetComponentData(componentData.data, componentData.typeId, this.currentHandle.entity);
            }
        }

        public void SetAddComponentSearchString(string value) {
            if (!this.addComponentSearchString.Equals(value)) {
                this.addComponentSearchString = value;
                this.UpdateAddComponentSuggestions();
                this.model.IncrementVersion();
            }
        }

        private void UpdateAddComponentSuggestions() {
            this.addComponentSuggestions.Clear();
            if (string.IsNullOrEmpty(this.addComponentSearchString)) {
                this.addComponentSuggestions.AddRange(this.storage.componentIds);
            }
            else {
                this.storage.GetComponentIdsMatchesWithPrefix(this.addComponentSearchString, this.addComponentSuggestions);
            }

            foreach (var component in this.model.components) {
                var id = this.storage.GetModel().typeIdToInternalId[component.typeId];
                this.model.addComponentSuggestions.Remove(id);
            }
        }
    }
}
#endif