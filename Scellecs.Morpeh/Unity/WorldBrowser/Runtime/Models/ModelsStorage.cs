#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class ModelsStorage : IModelsStorage {
        private ComponentStorage componentStorage;
        private HierarchySearch hierarchySearch;
        private Hierarchy hierarchy;
        private Inspector inspector;

        public IComponentStorageProcessor ComponentStorageProcessor => this.componentStorage;
        public IHierarchySearchProcessor HierarchySearchProcessor => this.hierarchySearch;
        public IHierarchyProcessor HierarchyProcessor => this.hierarchy;
        public IInspectorProcessor InspectorProcessor => this.inspector;

        public bool Initialize() {
            try {
                this.componentStorage = new ComponentStorage();
                this.hierarchySearch = new HierarchySearch(this.componentStorage);
                this.hierarchy = new Hierarchy(this.hierarchySearch);
                this.inspector = new Inspector(this.hierarchy, this.componentStorage);
            }
            catch (Exception e) {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        public void Update() {
            this.componentStorage.ValidateUpdateCache();
            this.hierarchySearch.Update();
            this.hierarchy.Update();
            this.inspector.Update();
        }

        public void Dispose() { 
            
        }
    }
}
#endif