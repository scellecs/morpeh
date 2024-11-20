#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class Inspector {
        private readonly Hierarchy hierarchy;
        private readonly ComponentsStorage componentStorage;
        private readonly List<ComponentData> components;
        private readonly HashSet<int> expandedStates;

        private EntityHandle currentHandle;
        private long version;

        internal Inspector(Hierarchy hierarchy, ComponentsStorage componentStorage) {
            this.hierarchy = hierarchy;
            this.componentStorage = componentStorage;
            this.components = new List<ComponentData>();
            this.expandedStates = new HashSet<int>();
            this.currentHandle = default;
            this.version = 0u;
        }

        internal void Update() {
            var handle = this.hierarchy.GetSelectedEntityHandle();
            if (!this.currentHandle.IsValid && !handle.IsValid) {
                if (this.components.Count > 0) {
                    this.UpdateComponentsList();
                }
                return;
            }

            if (!this.currentHandle.ArchetypesEqual(handle)) {
                if (!this.currentHandle.EntitiesEqual(handle)) {
                    this.SetExpandedAll(false);
                }
                this.currentHandle = handle;
                this.UpdateComponentsList();
            }
        }

        internal IList GetItemsSource() {
            return this.components;
        }

        internal long GetVersion() {
            return this.version;
        }

        internal bool IsNotExpandedAll() {
            return this.expandedStates.Count < this.components.Count;
        }

        internal bool IsExpanded(int typeId) {
            return this.expandedStates.Contains(typeId);
        }

        internal void SetExpanded(int typeId, bool value) {
            if (value) {
                this.expandedStates.Add(typeId);
            }
            else {
                this.expandedStates.Remove(typeId);
            }
        }

        internal void SetExpandedAll(bool value) {
            if (value) {
                foreach (var componentData in this.components) {
                    this.expandedStates.Add(componentData.TypeId);
                }
            }
            else {
                this.expandedStates.Clear();
            }

            this.IncrementVersion();
        }

        private void UpdateComponentsList() {
            this.IncrementVersion();
            this.components.Clear();

            if (!this.currentHandle.IsValid) {
                this.expandedStates.Clear();
                return;
            }

            var archetypeComponents = this.currentHandle.Archetype.components;
            foreach(var typeId in archetypeComponents) {
                this.components.Add(new ComponentData() {
                    internalTypeDefinition = ExtendedComponentId.Get(typeId),
                    niceName = this.componentStorage.GetComponentNameByTypeId(typeId),
                    entity = this.currentHandle.entity
                });
            }
        }

        private void IncrementVersion() {
            unchecked {
                this.version++;
            }
        }
    }
}
#endif