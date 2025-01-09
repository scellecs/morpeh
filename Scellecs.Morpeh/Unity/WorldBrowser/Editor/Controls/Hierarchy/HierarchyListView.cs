#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchyListView : ListView {
        private readonly IHierarchyViewModel model;
        private readonly Stack<HierarchyListViewItem> pool;

        internal HierarchyListView(IHierarchyViewModel model) {
            this.model = model;
            this.pool = new Stack<HierarchyListViewItem>();

            this.selectionType = SelectionType.Single;
            this.makeItem = () => this.Rent();
            this.bindItem = (e, i) => {
                var entity = (this.itemsSource as IList<Entity>)[i];
                var item = (HierarchyListViewItem)e;
                item.Bind(entity);
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
                    var index = indices.First();
                    var entity = (this.itemsSource as IList<Entity>)[index];
                    this.model.SetSelectedEntity(entity);
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

