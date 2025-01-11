#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer;
using System;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class InspectorView : VisualElement, IDisposable {
        private readonly InspectorToolbar toolbar;
        private readonly ExpandFoldoutToggle expandAllToggle;
        private readonly AddComponentMenu addComponentMenu;
        private readonly InspectorListView listView;
        private readonly ViewContainer viewContainer;

        private readonly IInspectorViewModel model;

        private long modelVersion;

        internal InspectorView(IInspectorViewModel model) {
            this.model = model;

            this.expandAllToggle = new ExpandFoldoutToggle("Expand All");
            this.expandAllToggle.ValueChanged += (evt) => this.model.SetExpandedAll(evt.newValue);
            this.addComponentMenu = new AddComponentMenu(this.model);

            this.toolbar = new InspectorToolbar();
            this.toolbar.Add(this.expandAllToggle);
            this.toolbar.Add(this.addComponentMenu);

            this.listView = new InspectorListView(this.model);

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
            if (this.model.IsNotExpandedAll()) {
                this.expandAllToggle.SetValueWithoutNotify(false);
            }

            this.addComponentMenu.Refresh();
            this.listView.UpdateItems();
            this.modelVersion = this.model.GetVersion();
        }

        public void Dispose() {
            this.listView.Dispose();
        }
    }
}
#endif