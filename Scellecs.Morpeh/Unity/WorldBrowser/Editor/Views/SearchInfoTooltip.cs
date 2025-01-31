#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class SearchInfoTooltip : VisualElement {
        private const string TOOLTIP_ICON = "hierarchy-search-tooltip-icon";
        private const string TOOLTIP_LABEL = "hierarchy-search-tooltip-label";

        private readonly Label tooltipLabel;

        internal SearchInfoTooltip() {
            this.AddToClassList(TOOLTIP_ICON);

            this.tooltipLabel = new Label();
            this.tooltipLabel.AddToClassList(TOOLTIP_LABEL);
            this.tooltipLabel.style.display = DisplayStyle.None;

            this.Add(tooltipLabel);
        }

        internal void AddTooltipHandler(string message) {
            this.RegisterCallback<MouseEnterEvent>((evt) => {
                var position = evt.localMousePosition + new Vector2(-260, -30);
                this.tooltipLabel.text = message;
                this.tooltipLabel.style.left = position.x;
                this.tooltipLabel.style.top = position.y;
                this.tooltipLabel.style.display = DisplayStyle.Flex;
            });

            this.RegisterCallback<MouseLeaveEvent>((evt) => {
                this.tooltipLabel.style.display = DisplayStyle.None;
            });
        }
    }
}
#endif