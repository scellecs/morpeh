#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Scellecs.Morpeh.WorldBrowser.Filter;
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class Inspector : IInspectorProcessor {
        private readonly InspectorModel model;
        private readonly Hierarchy hierarchy;
        private readonly ComponentStorage storage;

        private EntityHandle currentHandle;

        internal Inspector(Hierarchy hierarchy, ComponentStorage storage) {
            this.model = new InspectorModel();
            this.model.components = new List<ComponentDataBoxed>();
            this.hierarchy = hierarchy;
            this.storage = storage;
            this.currentHandle = default;
        }

        internal void Update() {
            var handle = this.hierarchy.GetSelectedEntityHandle();
            if (!this.currentHandle.IsValid && !handle.IsValid) {
                if (this.model.components.Count > 0) {
                    this.model.components.Clear();
                    this.model.selectedEntity = default;
                    this.model.IncrementVersion();
                    this.currentHandle = EntityHandle.Invalid;
                }

                return;
            }

            if (!this.currentHandle.Equals(handle)) {
                this.currentHandle = handle;
                this.model.selectedEntity = this.currentHandle.entity;
                this.model.IncrementVersion();
            }

            this.storage.FetchEntityComponents(this.currentHandle.entity, this.model.components);
        }

        public InspectorModel GetModel() {
            return this.model;
        }

        public void SetComponentData(int typeId, object data) {
            this.storage.SetComponentData(data, typeId, this.currentHandle.entity);
        }
    }
}
#endif