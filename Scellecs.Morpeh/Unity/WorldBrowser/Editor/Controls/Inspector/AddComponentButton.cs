#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class AddComponentButton : VisualElement {
        private const string ADD_BUTTON = "inspector-add-component-menu-button";
        private const string ADD_BUTTON_TEXT = "inspector-add-component-menu-button__text";
        private const string ADD_BUTTON_ICON = "inspector-add-component-menu-button__icon";

        private readonly VisualElement icon;
        private readonly Label label;

        internal event Action OnClick;

        internal AddComponentButton(string text) {
            this.AddToClassList(ADD_BUTTON);

            this.icon = new VisualElement();
            this.icon.AddToClassList(ADD_BUTTON_ICON);

            this.label = new Label(text);
            this.label.AddToClassList(ADD_BUTTON_TEXT);

            this.Add(this.icon);
            this.Add(this.label);
            this.RegisterCallback<ClickEvent>(_ => this.OnClick?.Invoke());
        }
    }
}
#endif