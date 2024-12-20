﻿#if UNITY_EDITOR
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
                    this.components.Clear();
                    this.expandedStates.Clear();
                    this.currentHandle = default;
                    this.IncrementVersion();
                }
                return;
            }

            if (!this.currentHandle.Equals(handle)) {
                if (!this.currentHandle.EntitiesEqual(handle)) {
                    this.SetExpandedAll(false);
                }

                this.currentHandle = handle;
                this.UpdateComponentsList();
                this.IncrementVersion();
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
            this.expandedStates.Clear();

            if (value) {
                foreach (var componentData in this.components) {
                    this.expandedStates.Add(componentData.TypeId);
                }
            }

            this.IncrementVersion();
        }

        private void UpdateComponentsList() {
            this.components.Clear();
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