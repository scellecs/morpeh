#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class ViewContainer : VisualElement {
        private const string TOOLBAR_CONTAINER = "view-container-toolbar-container";
        private const string LIST_CONTAINER = "view-container-list-container";

        private readonly VisualElement toolbarContainer;
        private readonly VisualElement listContainer;

        internal ViewContainer() {
            this.toolbarContainer = new VisualElement();
            this.listContainer = new VisualElement();

            this.toolbarContainer.AddToClassList(TOOLBAR_CONTAINER);
            this.listContainer.AddToClassList(LIST_CONTAINER);

            this.Add(toolbarContainer);
            this.Add(listContainer);
        }

        internal void SetToolbar(VisualElement toolbar) {
            this.toolbarContainer.Clear();
            this.toolbarContainer.Add(toolbar);
        }

        internal void SetListView(VisualElement listView) {
            this.listContainer.Clear();
            this.listContainer.Add(listView);
        }
    }
}
#endif