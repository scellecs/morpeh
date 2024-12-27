#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Filter;
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class Hierarchy : IHierarchyProcessor {
        private readonly HierarchyModel model;
        private readonly SearchFilter filter;
        private readonly SearchFilterData searchData;
        private readonly HierarchySearch hierarchySearch;

        private readonly List<World> worlds;
        private readonly List<int> worldIdentifiers;
        private readonly HashSet<int> selectedWorlds;
        private readonly Dictionary<int, World> worldsMap;

        private EntityHandle selectedHandle;
        private long hierarchySearchVersion;
        private bool isDefaultWorldInitialized;

        internal Hierarchy(HierarchySearch hierarchySearch) {
            this.model = new HierarchyModel();
            this.hierarchySearch = hierarchySearch;
            this.filter = new SearchFilter();
            this.searchData = new SearchFilterData();
            this.worlds = new List<World>();
            this.worldIdentifiers = new List<int>();
            this.selectedWorlds = new HashSet<int>();
            this.worldsMap = new Dictionary<int, World>();
            this.selectedHandle = default;
            this.hierarchySearchVersion = -1u;
            this.isDefaultWorldInitialized = false;
            this.InitDeafautWorld();
            this.UpdateModel();
        }

        internal void InitDeafautWorld() {
            if (!this.isDefaultWorldInitialized) {
                var defaultWorld = World.Default;
                if (!defaultWorld.IsNullOrDisposed()) {
                    this.worlds.Add(defaultWorld);
                    this.worldIdentifiers.Add(defaultWorld.identifier);
                    this.selectedWorlds.Add(defaultWorld.identifier);
                    this.isDefaultWorldInitialized = true;
                }
            }
        }

        internal void Update() {
            this.InitDeafautWorld();

            if (this.hierarchySearchVersion != this.hierarchySearch.GetVersion()) {
                this.hierarchySearchVersion = this.hierarchySearch.GetVersion();
                this.hierarchySearch.FillSearchFilterData(this.searchData);
            }

            this.RemapWorlds();
            this.filter.Update(this.searchData, this.worlds);
            this.SetSelectedEntity(this.selectedHandle.entity);
            this.UpdateModel();
        }

        internal EntityHandle GetSelectedEntityHandle() {
            return this.selectedHandle;
        }

        public HierarchyModel GetModel() { 
            return this.model; 
        }

        public void SetSelectedEntity(Entity entity) {
            this.selectedHandle = new EntityHandle(entity);
            this.UpdateModel();
        }

        public void SetSelectedWorldId(int id, bool state) {
            if (state) {
                this.selectedWorlds.Add(id);
            }
            else {
                this.selectedWorlds.Remove(id);
            }

            this.UpdateModel();
        }

        private void RemapWorlds() {
            var globalWorlds = World.worlds;
            var globalWorldsCount = World.worldsCount;

            this.worldsMap.Clear();
            this.worlds.Clear();
            this.worldIdentifiers.Clear();
            for (int i = 0; i < globalWorldsCount; i++)  {
                var world = globalWorlds[i];
                this.worldsMap[world.identifier] = world;
                this.worldIdentifiers.Add(world.identifier);
            }

            this.selectedWorlds.RemoveWhere(identifier => !this.worldsMap.ContainsKey(identifier));
            foreach (var identifier in this.selectedWorlds) {
                if (this.worldsMap.TryGetValue(identifier, out var world)) {
                    this.worlds.Add(world);
                }
            }
        }

        private void UpdateModel() {
            this.model.worldIds = this.worldIdentifiers;
            this.model.selectedWorldIds = this.selectedWorlds;
            this.model.entities = this.filter.searchResult;
            this.model.IncrementVersion();
        }
    }
}
#endif
