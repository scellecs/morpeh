#if UNITY_EDITOR
using Scellecs.Morpeh.Collections;
using System.Buffers;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class SearchFilterWorldState {
        internal readonly ArrayPool<int> arrayPool;
        internal readonly HashSet<long> archetypesHashes;
        internal readonly HashSet<long> deletedArchetypes;
        internal readonly HashSet<long> filteredArchetypes;
        internal readonly IntHashMap<ComponentToArchetypes> componentsToArchetypes;
        internal readonly LongHashMap<ArchetypeToComponents> archetypesToComponents;
        internal World world;

        internal SearchFilterWorldState() {
            this.arrayPool = ArrayPool<int>.Create();
            this.archetypesHashes = new HashSet<long>();
            this.deletedArchetypes = new HashSet<long>();
            this.filteredArchetypes = new HashSet<long>();
            this.componentsToArchetypes = new IntHashMap<ComponentToArchetypes>();
            this.archetypesToComponents = new LongHashMap<ArchetypeToComponents>();
            this.world = default;
        }

        internal void Reset() {
            foreach (var idx in this.archetypesToComponents) {
                var components = archetypesToComponents.GetValueByIndex(idx);
                this.arrayPool.Return(components.value);
            }

            this.archetypesHashes.Clear();
            this.deletedArchetypes.Clear();
            this.filteredArchetypes.Clear();
            this.componentsToArchetypes.Clear();
            this.archetypesToComponents.Clear();
            this.world = default;
        }

        internal void FetchArchetypes() {
            deletedArchetypes.Clear();
            var worldArchetypes = world.archetypes;
            foreach (var archetypeHash in this.archetypesHashes) {
                if (!worldArchetypes.Has(archetypeHash)) {
                    this.deletedArchetypes.Add(archetypeHash);
                    RemoveArchetypeFromComponents(archetypeHash);
                }
            }

            this.archetypesHashes.ExceptWith(deletedArchetypes);

            foreach (var idx in worldArchetypes) {
                var archetype = worldArchetypes.GetValueByIndex(idx);
                var archetypeHash = archetype.hash.GetValue();

                if (this.archetypesHashes.Add(archetypeHash)) { 
                    UpdateComponentsForArchetype(archetype, archetypeHash);
                }
            }

            void RemoveArchetypeFromComponents(long archetypeHash) {
                if (this.archetypesToComponents.TryGetValue(archetypeHash, out var components)) {
                    foreach (var typeId in components.value) {
                        if (this.componentsToArchetypes.TryGetValue(typeId, out var archetypes)) {
                            archetypes.value.Remove(archetypeHash);
                        }
                    }
                }
            }

            void UpdateComponentsForArchetype(Archetype archetype, long archetypeHash) {
                var components = new ArchetypeToComponents() { value = this.arrayPool.Rent(archetype.components.length) };
                var index = 0;
                foreach (var typeId in archetype.components) {
                    components.value[index++] = typeId;
                    if (!this.componentsToArchetypes.TryGetValue(typeId, out var archetypes)) {
                        archetypes = new ComponentToArchetypes { value = new HashSet<long>() };
                        this.componentsToArchetypes.Add(typeId, archetypes, out _);
                    }
                    archetypes.value.Add(archetypeHash);
                }
                this.archetypesToComponents.Add(archetypeHash, components, out _);
            }
        }

        internal void FilterArchetypes(List<int> inc, List<int> exc) {
            this.filteredArchetypes.Clear();

            if (inc.Count == 0) {
                this.filteredArchetypes.UnionWith(archetypesHashes);
            }
            else {
                var firstIncluded = inc[0];
                if (this.componentsToArchetypes.TryGetValue(firstIncluded, out var initialArchetypes)) {
                    this.filteredArchetypes.UnionWith(initialArchetypes.value);
                }
                else {
                    return;
                }

                for (int i = 1; i < inc.Count; i++) {
                    var included = inc[i];
                    if (this.componentsToArchetypes.TryGetValue(included, out var archetypes)) {
                        this.filteredArchetypes.IntersectWith(archetypes.value);
                    }
                    else {
                        this.filteredArchetypes.Clear();
                        return;
                    }
                }
            }

            foreach (var excluded in exc) {
                if (this.componentsToArchetypes.TryGetValue(excluded, out var archetypes)) {
                    this.filteredArchetypes.ExceptWith(archetypes.value);
                }
            }
        }
    }
}
#endif