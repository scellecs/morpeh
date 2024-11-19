#if UNITY_EDITOR
using UnityEngine.UIElements;
using HierarchyModel = Scellecs.Morpeh.Utils.Editor.Hierarchy;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class HierarchyToolbar : VisualElement {
        private const string TOOLBAR = "hierarchy-toolbar";
        private const string DROPDOWN = "hierarchy-world-selector-dropdown";

        private readonly WorldSelectorListView listView;
        private readonly HierarchyModel model;
        private readonly VisualElement dropdownButton;

        private bool isExpanded;

        internal HierarchyToolbar(HierarchyModel model) {
            this.model = model;
            this.AddToClassList(TOOLBAR);

            this.dropdownButton = new VisualElement();
            this.dropdownButton.AddToClassList(DROPDOWN);

            var buttonText = new Label("Select Worlds");
            this.dropdownButton.Add(buttonText);

            this.dropdownButton.AddManipulator(new Clickable(ToggleDropdown));

            this.listView = new WorldSelectorListView(this.model);
            this.listView.style.display = DisplayStyle.None;

            this.Add(this.dropdownButton);
            this.Add(this.listView);
        }

        private void ToggleDropdown() {
            this.isExpanded = !this.isExpanded;
            this.listView.style.display = this.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            if (this.isExpanded) {
                this.listView.UpdateItemsSource();
            }
        }

        internal void Update() {
            if (this.isExpanded) {
                this.listView.UpdateItemsSource();
            }
        }
    }
}
#endif