#if UNITY_EDITOR || DEVELOPMENT_BUILD
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

        internal Hierarchy(HierarchySearch hierarchySearch) {
            this.model = new HierarchyModel();
            this.hierarchySearch = hierarchySearch;
            this.filter = new SearchFilter();
            this.searchData = new SearchFilterData();
            this.worlds = new List<World>() { World.Default };
            this.worldIdentifiers = new List<int>() { World.Default.identifier };
            this.selectedWorlds = new HashSet<int>() { World.Default.identifier };
            this.worldsMap = new Dictionary<int, World>();
            this.hierarchySearchVersion = -1u;
            this.UpdateModel();
        }

        internal void Update() {
            if (this.hierarchySearchVersion != this.hierarchySearch.GetVersion()) {
                this.hierarchySearchVersion = this.hierarchySearch.GetVersion();
                this.hierarchySearch.FillSearchFilterData(this.searchData);
            }

            this.RemapWorlds();
            this.filter.Update(this.searchData, this.worlds);
            this.UpdateModel();
        }

        internal EntityHandle GetSelectedEntityHandle() {
            return this.selectedHandle;
        }

        public HierarchyModel GetModel() { 
            return this.model; 
        }

        public void SetSelectedEntity(Entity entity) {
            if (EntityHandleUtils.IsValid(entity, entity.GetWorld())) {
                var archetype = EntityHandleUtils.GetArchetype(entity);
                this.selectedHandle = new EntityHandle(entity, archetype.hash.GetValue());
                this.UpdateModel();
            }
            else {
                this.selectedHandle = EntityHandle.Invalid;
            }
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

        private void UpdateModel() {
            this.model.worldIds = this.worldIdentifiers;
            this.model.selectedWorldIds = this.selectedWorlds;
            this.model.entities = this.filter.searchResult;
            this.model.IncrementVersion();
        }
    }
}
#endif
