#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchySearchListView : ListView {
        private readonly IHierarchySearchViewModel model;
        private readonly QueryParam queryParam;
        private readonly Stack<HierarchySearchListViewItem> pool;

        internal HierarchySearchListView(IHierarchySearchViewModel model, QueryParam param) {
            this.model = model;
            this.queryParam = param;
            this.pool = new Stack<HierarchySearchListViewItem>();

            this.selectionType = SelectionType.None;
            this.makeItem = () => this.Rent();
            this.bindItem = (e, i) => {
                var id = (this.itemsSource as IList<int>)[i];
                var item = (HierarchySearchListViewItem)e;
                item.Bind(id);
            };
            this.unbindItem = (e, i) => {
                var item = (HierarchySearchListViewItem)e;
                item.Reset();
            };
            this.destroyItem = (e) => {
                var item = (HierarchySearchListViewItem)e;
                item.Reset();
                this.Return(item);
            };
        }

        internal void UpdateItemsSource() {
            this.itemsSource = null;
            this.Clear();
            this.itemsSource = this.model.GetItemsSource(this.queryParam);
            this.Rebuild();
        }

        private HierarchySearchListViewItem Rent() {
            return this.pool.Count > 0 ? this.pool.Pop() : new HierarchySearchListViewItem(this.model, this.queryParam);
        }

        private void Return(HierarchySearchListViewItem item) {
            this.pool.Push(item);
        }
    }
}
#endif