#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.Utils;
using System.Collections;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchyViewModel : BaseViewModel, IHierarchyViewModel {
        private readonly HierarchyModel model;
        private readonly IHierarchyProcessor processor;

        private readonly VirtualList<int> worldsList;
        private readonly VirtualList<Entity> entitiesList;

        private Entity lastSelectedEntity;
        private long modelVersion;

        internal HierarchyViewModel(IHierarchyProcessor processor) {
            this.model = processor.GetModel();
            this.processor = processor;
            this.worldsList = new VirtualList<int>(this.model.worldIds);
            this.entitiesList = new VirtualList<Entity>(this.model.entities);
            this.version = 0u;
            this.modelVersion = -1u;
        }

        public void Update() {
            if (this.modelVersion != this.model.version) {
                this.modelVersion = this.model.version;
                this.worldsList.SetList(this.model.worldIds);
                this.entitiesList.SetList(this.model.entities);
                this.IncrementVersion();
            }
        }

        public bool IsSelectedWorldId(int id) {
            return this.model.selectedWorldIds.Contains(id);
        }

        public int GetSelectedEntityIndex() {
            return this.model.entities.IndexOf(this.lastSelectedEntity);
        }

        public int GetTotalEntitiesFound() { 
            return this.model.entities.Count;
        }

        public IList GetWorldsSource() {
            return this.worldsList;
        }

        public IList GetEntitiesSource() {
            return this.entitiesList;
        }

        public void SetSelectedWorldId(int id, bool state) {
            this.processor.SetSelectedWorldId(id, state);
        }

        public void SetSelectedEntity(Entity entity) {
            this.lastSelectedEntity = entity;
            this.processor.SetSelectedEntity(entity);
        }
    }
}
#endif
