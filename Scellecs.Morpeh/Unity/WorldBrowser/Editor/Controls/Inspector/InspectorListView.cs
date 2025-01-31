#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class InspectorListView : ListView, IDisposable {
        private readonly IInspectorViewModel model;

        private readonly List<InspectorListViewItem> visibleItems;
        private readonly List<ComponentViewHandle> handles;
        private readonly Stack<InspectorListViewItem> pool;

        internal InspectorListView(IInspectorViewModel model) {
            this.model = model;
            this.visibleItems = new List<InspectorListViewItem>();
            this.handles = new List<ComponentViewHandle>();
            this.pool = new Stack<InspectorListViewItem>();

            this.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            this.selectionType = SelectionType.None;
            this.horizontalScrollingEnabled = false;

            this.makeItem = () => this.Rent();
            this.bindItem = (e, i) => {
                var typeId = (this.itemsSource as IList<int>)[i];
                var item = (InspectorListViewItem)e;
                item.Bind(typeId);
                this.visibleItems.Add(item);
            };
            this.unbindItem = (e, i) => {
                var item = (InspectorListViewItem)e;
                item.Reset();
                this.visibleItems.Remove(item);
            };
            this.destroyItem = (e) => {
                var item = (InspectorListViewItem)e;
                item.Reset();
                this.Return(item);
                this.visibleItems.Remove(item);
            };

            this.itemsSource = this.model.GetEntityComponentsSource();
        }

        internal void UpdateItems() {
            this.RefreshItems();

            foreach (var visible in this.visibleItems) {
                visible.Refresh();
            }
        }

        protected override CollectionViewController CreateViewController() {
            return new InspectorListViewController(this.model);
        }

        private InspectorListViewItem Rent() {
            if (pool.Count > 0) {
                return this.pool.Pop();
            }

            var handle = ComponentViewHandle.Create();
            this.handles.Add(handle);
            return new InspectorListViewItem(this.model, handle);
        }

        private void Return(InspectorListViewItem item) {
            this.pool.Push(item);
        }

        public void Dispose() {
            foreach (var handle in this.handles) {
                handle.Dispose();
            }
        }
    }
}
#endif