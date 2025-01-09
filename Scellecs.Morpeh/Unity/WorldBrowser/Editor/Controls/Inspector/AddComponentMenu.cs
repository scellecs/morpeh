#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class AddComponentMenu : VisualElement {
        private const string MENU = "inspector-add-component-menu";
        private const string DROPDOWN = "inspector-add-component-menu-dropdown";
        private const string MENU_CONTENT = "inspector-add-component-menu-dropdown-menu-content";
        private const string INPUT_FIELD = "inspector-add-component-menu-input";

        private readonly IInspectorViewModel model;
        private readonly VisualElement content;
        private readonly TextField inputField;
        private readonly AddComponentListView addComponentListView;
        private readonly PopupContainer addComponentMenu;
        private readonly AddComponentButton addComponentButton;

        internal AddComponentMenu(IInspectorViewModel model) {
            this.model = model;
            this.AddToClassList(MENU);

            this.addComponentMenu = new PopupContainer();
            this.addComponentMenu.AddToClassList(DROPDOWN);

            this.content = new VisualElement();
            this.content.AddToClassList(MENU_CONTENT);

            this.inputField = new TextField();
            this.inputField.AddToClassList(INPUT_FIELD);
            this.inputField.RegisterValueChangedCallback((evt) => this.model.SetAddComponentSearchString(evt.newValue));

            this.addComponentListView = new AddComponentListView(this.model);

            this.content.Add(this.inputField);
            this.content.Add(this.addComponentListView);
            this.addComponentMenu.ContentContainer.Add(content);

            this.addComponentMenu.ContentContainer.style.overflow = Overflow.Hidden;
            this.addComponentMenu.ContentContainer.style.width = 300;
            this.addComponentMenu.ContentContainer.style.height = 400;

            this.addComponentButton = new AddComponentButton("Add Component");
            this.addComponentButton.OnClick += () => {
                if (this.addComponentListView.itemsSource.Count > 0) {
                    var rect = this.addComponentButton.worldBound;
                    this.addComponentMenu.Show(this.addComponentButton, rect);
                }
            };

            this.Add(this.addComponentButton);
        }

        internal void Refresh() {
            this.addComponentListView.RefreshItems();
        }
    }
}
#endif