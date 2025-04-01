#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class AddComponentListView : ListView {
        private const string LIST = "inspector-add-component-menu-list";

        private readonly IInspectorViewModel model;
        private readonly Stack<AddComponentListViewItem> pool;

        internal AddComponentListView(IInspectorViewModel model) {
            this.model = model;
            this.pool = new Stack<AddComponentListViewItem>();
            this.AddToClassList(LIST);

            this.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            this.selectionType = SelectionType.None;
            this.horizontalScrollingEnabled = false;

            this.makeItem = () => this.Rent();
            this.bindItem = (e, i) => {
                var typeId = (this.itemsSource as IList<int>)[i];
                var item = (AddComponentListViewItem)e;
                item.Bind(typeId);
            };
            this.unbindItem = (e, i) => {
                var item = (AddComponentListViewItem)e;
                item.Reset();
            };
            this.destroyItem = (e) => {
                var item = (AddComponentListViewItem)e;
                item.Reset();
                this.Return(item);
            };

            this.itemsSource = this.model.GetAddComponentSuggestionsSource();
        }

        protected override CollectionViewController CreateViewController() {
            return new AddComponentListViewController(this.model);
        }

        private AddComponentListViewItem Rent() {
            return this.pool.Count > 0 ? this.pool.Pop() : new AddComponentListViewItem(this.model);
        }

        private void Return(AddComponentListViewItem item) {
            this.pool.Push(item);
        }

    }
}
#endif