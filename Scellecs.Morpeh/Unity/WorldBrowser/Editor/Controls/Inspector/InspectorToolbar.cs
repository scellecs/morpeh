#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class InspectorToolbar : VisualElement {
        private const string TOOLBAR = "inspector-toolbar";

        internal InspectorToolbar() {
            this.AddToClassList(TOOLBAR);
        }
    }
}
#endif