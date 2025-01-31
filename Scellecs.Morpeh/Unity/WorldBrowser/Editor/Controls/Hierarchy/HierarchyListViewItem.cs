#if UNITY_EDITOR
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchyListViewItem : VisualElement {
        private const string ITEM = "hierarchy-list-view-item";
        private const string LEFT_HALF = "hierarchy-list-view-left-half";
        private const string RIGHT_HALF = "hierarchy-list-view-right-half";

        private readonly Label leftLabel;
        private readonly Label rightLabel;

        internal HierarchyListViewItem() {
            this.AddToClassList(ITEM);

            var leftHalf = new VisualElement();
            leftHalf.AddToClassList(LEFT_HALF);

            var rightHalf = new VisualElement();
            rightHalf.AddToClassList(RIGHT_HALF);

            this.leftLabel = new Label();
            this.rightLabel = new Label();

            leftHalf.Add(this.leftLabel);
            rightHalf.Add(this.rightLabel);

            this.Add(leftHalf);
            this.Add(rightHalf);
        }

        internal void Bind(Entity entity) {
            this.leftLabel.text = $"Entity: {entity.Id} : {entity.Generation}";
            this.rightLabel.text = $"World: {entity.WorldId} : {entity.WorldGeneration}";
        }

        internal void Reset() {
            this.leftLabel.text = string.Empty;
            this.rightLabel.text = string.Empty;
        }
    }
}
#endif