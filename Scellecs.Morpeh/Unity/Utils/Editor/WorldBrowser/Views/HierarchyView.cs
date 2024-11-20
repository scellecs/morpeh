#if UNITY_EDITOR
using System.Linq;
using UnityEngine.UIElements;
using HierarchyModel = Scellecs.Morpeh.Utils.Editor.Hierarchy;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class HierarchyView : VisualElement {
        private readonly HierarchyListView listView;
        private readonly HierarchyToolbar toolbar;
        private readonly ViewContainer viewContainer;

        private readonly HierarchyModel model;

        private long modelVersion;

        internal HierarchyView(HierarchyModel model) {
            this.model = model;
            this.toolbar = new HierarchyToolbar(this.model);
            this.listView = new HierarchyListView(this.model);

            this.viewContainer = new ViewContainer();
            this.viewContainer.SetToolbar(this.toolbar);
            this.viewContainer.SetListView(this.listView);

            this.Add(this.viewContainer);
            this.SyncWithModel();
        }

        internal void Update() {
            if (this.modelVersion != this.model.GetVersion()) {
                this.SyncWithModel();
            }
        }

        internal void SyncWithModel() {
            this.toolbar.Update();
            this.listView.UpdateItems();
            this.modelVersion = this.model.GetVersion();
        }
    }
}
#endif