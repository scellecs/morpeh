#if UNITY_EDITOR
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class SplitterView {
        internal readonly VisualElement root;
        internal readonly VisualElement hierarchySearchRoot;
        internal readonly VisualElement hierarchyRoot;
        internal readonly VisualElement inspectorRoot;

        internal SplitterView(int dim1, int dim2) {
            var splitView = new TwoPaneSplitView(1, dim1, TwoPaneSplitViewOrientation.Vertical);
            splitView.Add(this.hierarchyRoot = new VisualElement());
            splitView.Add(this.hierarchySearchRoot = new VisualElement());

            var splitView2 = new TwoPaneSplitView(0, dim2, TwoPaneSplitViewOrientation.Horizontal);
            splitView2.Add(splitView);
            splitView2.Add(this.inspectorRoot = new VisualElement());

            this.root = splitView2;
        }
    }
}
#endif
