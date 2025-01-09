#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class AddComponentListViewItem : VisualElement {
        private const string ITEM = "inspector-add-component-menu-list-item";
        private const string LABEL = "inspector-add-component-menu-list-item__label";

        private readonly IInspectorViewModel model;
        private readonly Label label;

        private int id;

        internal AddComponentListViewItem(IInspectorViewModel model) {
            this.model = model;
            this.id = -1;

            this.AddToClassList(ITEM);
            this.label = new Label();
            this.label.AddToClassList(LABEL);

            this.Add(this.label);
            this.AddManipulator(new Clickable(this.OnClick));
        }

        internal void Bind(int id) {
            this.id = id;
            this.label.text = this.model.GetComponentNameById(this.id);
        }

        internal void Reset() {
            this.id = -1;
            this.label.text = string.Empty;
        }

        private void OnClick() {
            if (this.id == -1) {
                return;
            }
            this.model.AddComponentById(this.id);
        }
    }
}
#endif