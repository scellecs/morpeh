#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchyView : VisualElement {
        private readonly HierarchyListView listView;
        private readonly HierarchyToolbar toolbar;
        private readonly ViewContainer viewContainer;

        private readonly IHierarchyViewModel model;
        private long modelVersion;

        internal HierarchyView(IHierarchyViewModel model) {
            this.model = model;
            this.toolbar = new HierarchyToolbar(this.model);
            this.listView = new HierarchyListView(this.model);

            this.viewContainer = new ViewContainer();
            this.viewContainer.SetToolbar(this.toolbar);
            this.viewContainer.SetListView(this.listView);

            this.Add(this.viewContainer);
            this.SyncWithViewModel();
        }

        internal void Update() {
            if (this.modelVersion != this.model.GetVersion()) {
                this.SyncWithViewModel();
            }
        }

        internal void SyncWithViewModel() {
            this.toolbar.Update();
            this.listView.UpdateItems();
            this.modelVersion = this.model.GetVersion();
        }
    }
}
#endif