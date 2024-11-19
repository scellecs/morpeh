#if UNITY_EDITOR
using Scellecs.Morpeh.Collections;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class SearchFilterWorldState {
        internal readonly HashSet<long> archetypesHashes;
        internal readonly HashSet<long> deletedArchetypes;
        internal readonly HashSet<long> filteredArchetypes;
        internal readonly IntHashMap<ComponentToArchetypes> componentsToArchetypes;
        internal readonly LongHashMap<ArchetypeToComponents> archetypesToComponents;

        internal World world;

        internal SearchFilterWorldState() {
            this.archetypesHashes = new HashSet<long>();
            this.deletedArchetypes = new HashSet<long>();
            this.filteredArchetypes = new HashSet<long>();
            this.componentsToArchetypes = new IntHashMap<ComponentToArchetypes>();
            this.archetypesToComponents = new LongHashMap<ArchetypeToComponents>();
            this.world = default;
        }

        internal void Reset() {
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
            foreach (var archetypeHash in archetypesHashes)
            {
                if (!worldArchetypes.Has(archetypeHash)) {
                    deletedArchetypes.Add(archetypeHash);
                    RemoveArchetypeFromComponents(archetypeHash);
                }
            }

            archetypesHashes.ExceptWith(deletedArchetypes);

            foreach (var idx in worldArchetypes) {
                var archetype = worldArchetypes.GetValueByIndex(idx);
                var archetypeHash = archetype.hash.GetValue();

                if (archetypesHashes.Add(archetypeHash)) { 
                    UpdateComponentsForArchetype(archetype, archetypeHash);
                }
            }

            void RemoveArchetypeFromComponents(long archetypeHash) {
                if (archetypesToComponents.TryGetValue(archetypeHash, out var components)) {
                    foreach (var typeId in components.value) {
                        if (componentsToArchetypes.TryGetValue(typeId, out var archetypes)) {
                            archetypes.value.Remove(archetypeHash);
                        }
                    }
                }
            }

            void UpdateComponentsForArchetype(Archetype archetype, long archetypeHash) {
                var components = new ArchetypeToComponents() { value = new int[archetype.components.length] }; //TODO
                var index = 0;
                foreach (var typeId in archetype.components) {
                    components.value[index++] = typeId;
                    if (!componentsToArchetypes.TryGetValue(typeId, out var archetypes)) {
                        archetypes = new ComponentToArchetypes { value = new HashSet<long>() }; //TODO
                        componentsToArchetypes.Add(typeId, archetypes, out _);
                    }
                    archetypes.value.Add(archetypeHash);
                }
                archetypesToComponents.Add(archetypeHash, components, out _);
            }
        }

        internal void FilterArchetypes(List<int> inc, List<int> exc) {
            this.filteredArchetypes.Clear();

            if (inc.Count == 0) {
                filteredArchetypes.UnionWith(archetypesHashes);
            }
            else {
                var firstIncluded = inc[0];
                if (componentsToArchetypes.TryGetValue(firstIncluded, out var initialArchetypes)) {
                    this.filteredArchetypes.UnionWith(initialArchetypes.value);
                }
                else {
                    return;
                }

                for (int i = 1; i < inc.Count; i++) {
                    var included = inc[i];
                    if (componentsToArchetypes.TryGetValue(included, out var archetypes)) {
                        this.filteredArchetypes.IntersectWith(archetypes.value);
                    }
                    else {
                        this.filteredArchetypes.Clear();
                        return;
                    }
                }
            }

            foreach (var excluded in exc) {
                if (componentsToArchetypes.TryGetValue(excluded, out var archetypes)) {
                    filteredArchetypes.ExceptWith(archetypes.value);
                }
            }
        }
    }
}
#endif