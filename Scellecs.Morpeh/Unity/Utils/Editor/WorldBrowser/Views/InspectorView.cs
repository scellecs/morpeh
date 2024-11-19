#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class InspectorView : VisualElement, IDisposable {
        private readonly InspectorToolbar toolbar;
        private readonly InspectorListView listView;
        private readonly ViewContainer viewContainer;

        private readonly ComponentViewHandle handle;
        private readonly Inspector model;

        private long modelVersion;

        internal InspectorView(Inspector model) {
            this.model = model;
            this.handle = ComponentViewHandle.Create();

            this.toolbar = new InspectorToolbar(this.model);
            this.listView = new InspectorListView(this.model, this.handle);

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
                this.toolbar.SetExpandedStateWithoutNotify(false);
            }

            this.listView.UpdateItems();
            this.modelVersion = this.model.GetVersion();
        }

        public void Dispose() {
            this.handle?.Dispose();
        }
    }
}
#endif