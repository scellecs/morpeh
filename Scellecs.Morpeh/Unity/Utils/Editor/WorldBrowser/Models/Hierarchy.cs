#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class Hierarchy {
        private readonly SearchFilter filter;
        private readonly SearchFilterData searchData;
        private readonly HierarchySearch hierarchySearch;

        private readonly List<World> worlds;
        private readonly List<int> worldIdentifiers;
        private readonly HashSet<int> selectedWorlds;
        private readonly Dictionary<int, World> worldsMap;

        private int selectedEntityIndex;
        private EntityHandle selectedHandle;

        private long hierarchySearchVersion;
        private long version;

        internal Hierarchy(HierarchySearch hierarchySearch) {
            this.hierarchySearch = hierarchySearch;
            this.filter = new SearchFilter();
            this.searchData = new SearchFilterData();
            this.worlds = new List<World>() { World.Default };
            this.worldIdentifiers = new List<int>() { World.Default.identifier };
            this.selectedWorlds = new HashSet<int>() { World.Default.identifier };
            this.worldsMap = new Dictionary<int, World>();
            this.selectedEntityIndex = -1;
            this.version = 0u;
            this.hierarchySearchVersion = -1u;
        }

        internal void Update() {
            if (this.hierarchySearchVersion != this.hierarchySearch.GetVersion()) {
                this.hierarchySearchVersion = this.hierarchySearch.GetVersion();
                this.hierarchySearch.FillSearchFilterData(this.searchData);
            }

            this.RemapWorlds();
            this.filter.Update(this.searchData, this.worlds);
            this.selectedEntityIndex = this.filter.searchResult[this.selectedHandle];

            unchecked {
                this.version++;
            }
        }

        internal EntityHandle GetSelectedEntityHandle() {
            return this.selectedHandle;
        }

        internal IList GetEntitiesSource() {
            return this.filter.searchResult;
        }

        internal List<int> GetWorldsSource() {
            return this.worldIdentifiers;
        }

        internal long GetVersion() {
            return this.version;
        }

        internal int GetSelectedEntityIndex() {
            return this.selectedEntityIndex;
        }

        internal bool IsSelectedWorldId(int id) {
            return this.selectedWorlds.Contains(id);
        }

        internal void SetSelectedEntityHandle(int index) {
            this.selectedHandle = index >= 0 ? this.filter.searchResult[index] : default;
            this.selectedEntityIndex = index;
        }

        internal void SetSelectedWorldId(int id, bool state) {
            if (state) {
                this.selectedWorlds.Add(id);
            }
            else {
                this.selectedWorlds.Remove(id);
            }
        }

        private void RemapWorlds() {
            var globalWorlds = World.worlds;
            var globalWorldsCount = World.worldsCount;

            this.worldsMap.Clear();
            this.worlds.Clear();
            for (int i = 0; i < globalWorldsCount; i++)  {
                var world = globalWorlds.data[i];
                this.worldsMap[world.identifier] = world;
            }

            this.selectedWorlds.RemoveWhere(identifier => !this.worldsMap.ContainsKey(identifier));

            foreach (var identifier in this.selectedWorlds) {
                if (this.worldsMap.TryGetValue(identifier, out var world)) {
                    this.worlds.Add(world);
                }
            }
        }
    }
}
#endif
