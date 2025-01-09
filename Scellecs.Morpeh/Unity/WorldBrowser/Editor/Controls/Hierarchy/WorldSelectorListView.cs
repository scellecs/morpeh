#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class WorldSelectorListView : ListView {
        private readonly IHierarchyViewModel model;
        private readonly Stack<WorldSelectorListViewItem> pool;

        internal WorldSelectorListView(IHierarchyViewModel model) {
            this.model = model;
            this.pool = new Stack<WorldSelectorListViewItem>();

            this.selectionType = SelectionType.None;
            this.makeItem = () => this.Rent();
            this.bindItem = (e, i) => {
                var worldId = (this.itemsSource as IList<int>)[i];
                var item = (WorldSelectorListViewItem)e;
                item.Bind(worldId);
            };
            this.unbindItem = (e, i) => {
                var item = (WorldSelectorListViewItem)e;
                item.Reset();
            };
            this.destroyItem = (e) => {
                var item = (WorldSelectorListViewItem)e;
                item.Reset();
                this.Return(item);
            };

            this.fixedItemHeight = 20;
            this.style.maxHeight = 300;
        }

        internal void UpdateItemsSource() {
            this.itemsSource = null;
            this.Clear();
            this.itemsSource = this.model.GetWorldsSource();
            this.Rebuild();
        }

        private WorldSelectorListViewItem Rent() {
            return this.pool.Count > 0 ? this.pool.Pop() : new WorldSelectorListViewItem(this.model);
        }

        private void Return(WorldSelectorListViewItem item) {
            this.pool.Push(item);
        }
    }
}
#endif