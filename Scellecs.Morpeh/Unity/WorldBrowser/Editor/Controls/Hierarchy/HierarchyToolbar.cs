#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchyToolbar : VisualElement {
        private const string TOOLBAR = "hierarchy-toolbar";
        private const string DROPDOWN = "hierarchy-world-selector-dropdown";
        private const string WORLDS_LABEL = "hierarchy-world-selector-dropdown-world-label";

        private readonly IHierarchyViewModel model;
        private readonly WorldSelectorListView listView;
        private readonly VisualElement dropdownButton;
        private readonly Label entitiesLabel;
        private readonly Label worldsLabel;

        private bool isExpanded;

        internal HierarchyToolbar(IHierarchyViewModel model) {
            this.model = model;
            this.AddToClassList(TOOLBAR);

            this.dropdownButton = new VisualElement();
            this.dropdownButton.AddToClassList(DROPDOWN);
            this.dropdownButton.AddManipulator(new Clickable(ToggleDropdown));

            this.entitiesLabel = new Label();
            this.dropdownButton.Add(entitiesLabel);

            this.worldsLabel = new Label("W");
            this.worldsLabel.AddToClassList(WORLDS_LABEL);
            this.dropdownButton.Add(this.worldsLabel);


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
            this.entitiesLabel.text = $"Entities Found: {this.model.GetTotalEntitiesFound()}";

            if (this.isExpanded) {
                this.listView.UpdateItemsSource();
            }
        }
    }
}
#endif