﻿#if UNITY_EDITOR
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class InspectorToolbar : VisualElement {
        private const string TOOLBAR = "inspector-toolbar";

        private readonly ExpandFoldoutToggle expandAllToggle;
        private readonly Inspector model;

        internal InspectorToolbar(Inspector model) {
            this.model = model;

            this.AddToClassList(TOOLBAR);
            this.expandAllToggle = new ExpandFoldoutToggle("Expand All");
            this.expandAllToggle.ValueChanged += (evt) => this.model.SetExpandedAll(evt.newValue);

            this.hierarchy.Add(this.expandAllToggle);
        }

        internal void SetExpandedStateWithoutNotify(bool state) { 
            this.expandAllToggle.SetValueWithoutNotify(state);
        }
    }
}
#endif