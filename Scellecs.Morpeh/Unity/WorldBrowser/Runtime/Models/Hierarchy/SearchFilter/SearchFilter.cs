#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal sealed class SearchFilter {
        private readonly List<SearchFilterWorldState> worldStates;
        private readonly List<SearchFilterWorldState> usedStates;
        private readonly Stack<SearchFilterWorldState> worldStatesPool;

        internal readonly SearchFilterList searchResult;

        internal SearchFilter() {
            this.worldStates = new List<SearchFilterWorldState>();
            this.usedStates = new List<SearchFilterWorldState>();
            this.worldStatesPool = new Stack<SearchFilterWorldState>();
            this.searchResult = new SearchFilterList();
        }

        internal void Update(SearchFilterData input, List<World> worlds) {
            if (!input.isValid) {
                this.searchResult.Clear();
                return;
            }

            this.UpdateWorldsList(worlds);

            if (input.ids.Count > 0) {
                this.FilterWorldsIds(input.ids);
            }
            else {
                this.FilterWorldsArchetypes(input.inc, input.exc);
            }
        }

        private void UpdateWorldsList(List<World> worlds) {
            this.usedStates.Clear();

            foreach (var world in worlds) {
                var found = default(SearchFilterWorldState);
                foreach (var worldState in this.worldStates) {
                    if (worldState.world == world) {
                        found = worldState; 
                        break;
                    }
                }

                if (found == default) {
                    found = this.Rent();
                    found.world = world;
                    this.worldStates.Add(found);
                }

                this.usedStates.Add(found);
            }

            var worldStatesCount = this.worldStates.Count;
            var writeIndex = 0;

            for (int readIndex = 0; readIndex < worldStatesCount; readIndex++) {
                var state = worldStates[readIndex];
                if (!state.world.IsNullOrDisposed()) {
                    worldStates[writeIndex] = state;
                    writeIndex++;
                }
                else {
                    state.Reset();
                    this.Return(state);
                }
            }

            if (writeIndex != worldStatesCount) {
                this.worldStates.RemoveRange(writeIndex, worldStatesCount - writeIndex);
            }
        }

        private void FilterWorldsIds(List<int> ids) {
            this.searchResult.Clear();
            this.searchResult.SetMode(SearchFilterListMode.EntityIds);
            foreach (var state in this.usedStates) {
                foreach (var id in ids) {
                    var entitesData = state.world.entities;
                    if (id > 0 && id < entitesData.Length) { 
                        var data = entitesData[id];
                        if (data.indexInCurrentArchetype == -1) {
                            continue;
                        }

                        var archetype = data.currentArchetype;
                        var entity = archetype.entities.data[data.indexInCurrentArchetype];
                        this.searchResult.Add(entity);
                    }
                }
            }
        }

        private void FilterWorldsArchetypes(List<int> inc, List<int> exc) {
            this.searchResult.Clear();
            this.searchResult.SetMode(SearchFilterListMode.Archetypes);
            foreach (var state in this.usedStates) {
                state.FetchArchetypes();
                state.FilterArchetypes(inc, exc);

                foreach (var stateArchetypeHash in state.filteredArchetypes) {
                    if (state.world.archetypes.TryGet(new ArchetypeHash(stateArchetypeHash), out var archetype)) {
                        this.searchResult.Add(archetype);
                    }
                }
            }
        }

        private SearchFilterWorldState Rent() {
            return this.worldStatesPool.Count > 0 ? this.worldStatesPool.Pop() : new SearchFilterWorldState();
        }

        private void Return(SearchFilterWorldState state) {
            this.worldStatesPool.Push(state);
        }
    }
}
#endif
