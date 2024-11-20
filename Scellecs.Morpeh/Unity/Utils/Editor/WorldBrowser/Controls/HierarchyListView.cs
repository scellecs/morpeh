#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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
            this.selectedIndicesChanged += (indices) => {
                if (indices.Any()) {
                    this.model.SetSelectedEntityHandle(indices.First());
                }
            };

            this.itemsSource = this.model.GetEntitiesSource();
        }

        internal void UpdateItems() {
            this.RefreshItems();

            var selection = this.model.GetSelectedEntityIndex();
            if (selection >= 0 && selection < this.itemsSource.Count) {
                this.selectedIndex = selection;
            }
            else if(this.selectedIndices.Any()) {
                this.ClearSelection();
            }
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

