#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchySearchListViewItem : VisualElement {
        private const string ITEM = "hierarchy-search-components-list-item";
        private const string CHECKMARK = "hierarchy-search-components-list-item__checkmark";
        private const string LABEL = "hierarchy-search-components-list-item__label";

        private readonly IHierarchySearchViewModel model;
        private readonly QueryParam queryParam;
        private readonly Label label;
        private readonly VisualElement checkmark;

        private int id;

        internal HierarchySearchListViewItem(IHierarchySearchViewModel model, QueryParam param) {
            this.model = model;
            this.queryParam = param;
            this.id = -1;

            this.AddToClassList(ITEM);

            this.checkmark = new VisualElement();
            this.checkmark.AddToClassList(CHECKMARK);

            this.label = new Label();
            this.label.AddToClassList(LABEL);

            this.Add(this.checkmark);
            this.Add(this.label);
            this.AddManipulator(new Clickable(this.OnClick));
        }

        internal void Bind(int id) {
            this.id = id;
            this.label.text = this.model.GetComponentNameById(this.id);
            this.UpdateVisualState(this.model.GetComponentIncluded(this.id, this.queryParam));
        }

        internal void Reset() {
            this.id = -1;
            this.label.text = string.Empty;
            this.UpdateVisualState(false);
        }

        private void OnClick() {
            if (this.id == -1) {
                return;
            }

            var newState = !this.model.GetComponentIncluded(this.id, this.queryParam);
            this.model.SetComponentIncluded(this.id, newState, this.queryParam);
            this.UpdateVisualState(newState);
        }

        private void UpdateVisualState(bool isSelected) {
            if (isSelected) {
                this.AddToClassList("selected");
                this.checkmark.AddToClassList("visible");
            }
            else {
                this.RemoveFromClassList("selected");
                this.checkmark.RemoveFromClassList("visible");
            }
        }
    }
}
#endif