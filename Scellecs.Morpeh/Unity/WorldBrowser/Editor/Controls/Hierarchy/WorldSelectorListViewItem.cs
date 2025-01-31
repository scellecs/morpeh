#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class WorldSelectorListViewItem : VisualElement {
        private const string ITEM = "world-selector-list-item";
        private const string CHECKMARK = "world-selector-list-item__checkmark";
        private const string LABEL = "world-selector-list-item__label";

        private readonly IHierarchyViewModel model;
        private readonly Label label;
        private readonly VisualElement checkmark;
        private int worldId;

        internal WorldSelectorListViewItem(IHierarchyViewModel model) {
            this.model = model;
            this.worldId = -1;

            this.AddToClassList(ITEM);

            this.checkmark = new VisualElement();
            this.checkmark.AddToClassList(CHECKMARK);

            this.label = new Label();
            this.label.AddToClassList(LABEL);

            this.Add(this.checkmark);
            this.Add(this.label);
            this.AddManipulator(new Clickable(this.OnClick));
        }

        internal void Bind(int worldId) {
            this.worldId = worldId;
            this.label.text = $"World {worldId}";
            this.UpdateVisualState(this.model.IsSelectedWorldId(worldId));
        }

        internal void Reset() {
            this.worldId = -1;
            this.label.text = string.Empty;
            this.UpdateVisualState(false);
        }

        private void OnClick() {
            var newState = !this.model.IsSelectedWorldId(this.worldId);
            this.model.SetSelectedWorldId(this.worldId, newState);
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