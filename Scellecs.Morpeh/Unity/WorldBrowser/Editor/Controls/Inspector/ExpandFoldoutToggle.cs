#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class ExpandFoldoutToggle : VisualElement {
        private const string EXPAND_TOGGLE = "expand-all-toggle";
        private const string EXPAND_TOGGLE_CHECKMARK = "expand-all-toggle__checkmark";
        private const string EXPAND_TOGGLE_TEXT = "expand-all-toggle__text";

        private readonly VisualElement checkmark;
        private readonly Label label;
        private bool value;

        internal event EventCallback<ChangeEvent<bool>> ValueChanged;

        internal bool Value {
            get => this.value;
            set {
                if (this.value != value) {
                    var evt = ChangeEvent<bool>.GetPooled(this.value, value);
                    this.value = value;
                    this.UpdateVisuals();
                    this.ValueChanged?.Invoke(evt);
                    evt.Dispose();
                }
            }
        }

        internal ExpandFoldoutToggle(string text) {
            this.AddToClassList(EXPAND_TOGGLE);

            this.checkmark = new VisualElement();
            this.checkmark.AddToClassList(EXPAND_TOGGLE_CHECKMARK);

            this.label = new Label(text);
            this.label.AddToClassList(EXPAND_TOGGLE_TEXT);

            this.Add(this.checkmark);
            this.Add(this.label);

            this.RegisterCallback<ClickEvent>(_ => this.Value = !this.Value);

            this.UpdateVisuals();
        }

        internal void SetValueWithoutNotify(bool value) {
            this.value = value;
            this.UpdateVisuals();
        }

        private void UpdateVisuals() {
            if (this.value) {
                this.checkmark.AddToClassList("expanded");
            }
            else {
                this.checkmark.RemoveFromClassList("expanded");
            }
        }
    }
}
#endif