#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;
using HierarchyModel = Scellecs.Morpeh.Utils.Editor.Hierarchy;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class HierarchyListView : ListView {
        private readonly HierarchyModel model;
        private readonly Stack<HierarchyListViewItem> pool;

        internal HierarchyListView(HierarchyModel model) {
            this.model = model;
            this.pool = new Stack<HierarchyListViewItem>();

            this.selectionType = SelectionType.Single;
            this.makeItem = () => this.Rent();
            this.bindItem = (e, i) => {
                var entityHandle = (this.itemsSource as SearchFilterList)[i];
                var item = (HierarchyListViewItem)e;
                item.Bind(entityHandle);
            };
            this.unbindItem = (e, i) => {
                var item = (HierarchyListViewItem)e;
                item.Reset();
            };
            this.destroyItem = (e) => {
                var item = (HierarchyListViewItem)e;
                item.Reset();
                this.Return(item);
            };

            this.itemsSource = this.model.GetEntitiesSource();
        }

        protected override CollectionViewController CreateViewController() {
            return new HierarchyListViewController(this.model);
        }

        private HierarchyListViewItem Rent() {
            return this.pool.Count > 0 ? this.pool.Pop() : new HierarchyListViewItem();
        }

        private void Return(HierarchyListViewItem item) {
            this.pool.Push(item);
        }
    }
}
#endif

