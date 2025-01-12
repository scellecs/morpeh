#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer;
using System;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class InspectorListViewItem : VisualElement {
        private const string ITEM = "inspector-list-view-item";
        private const string TAG_LABEL = "inspector-list-view-item__label";
        private const string DATA_FOLDOUT = "inspector-list-view-item__foldout";
        private const string DATA_CONTAINER = "inspector-list-view-item__data-container";
        private const string REMOVE_ICON = "inspector-list-view-item__remove-icon";

        private const string TAG_COLOR = "#90EE90";
        private const string TAG_FORMAT = "[Tag]";

        private const string NOT_SERIALIZED_COLOR = "#FFB6B6";
        private const string NOT_SERIALIZED_FORMAT = "[NotSerialized]";

        private readonly Label tagLabel;
        private readonly Foldout dataFoldout;
        private readonly Label labelRemoveIcon;
        private readonly Label foldoutRemoveIcon;
        private readonly IMGUIContainer dataContainer;
        private readonly Action IMGUIHandler;
        private readonly EventCallback<ChangeEvent<bool>> FoldoutExpandedCallback;
        private readonly ComponentViewHandle handle;
        private readonly IInspectorViewModel model;

        private ComponentData data;

        internal InspectorListViewItem(IInspectorViewModel model, ComponentViewHandle handle)
        {
            this.model = model;
            this.handle = handle;
            this.IMGUIHandler = () => this.handle.HandleOnGUI(this.data);
            this.FoldoutExpandedCallback = (evt) => this.model.SetExpanded(this.data.typeId, evt.newValue);

            this.AddToClassList(ITEM);

            this.tagLabel = new Label();
            this.tagLabel.AddToClassList(TAG_LABEL);

            this.labelRemoveIcon = new Label("×");
            this.labelRemoveIcon.AddToClassList(REMOVE_ICON);
            this.labelRemoveIcon.AddManipulator(new Clickable(() => {
                this.model.RemoveComponentByTypeId(this.data.typeId);
            }));

            this.foldoutRemoveIcon = new Label("×");
            this.foldoutRemoveIcon.AddToClassList(REMOVE_ICON);
            this.foldoutRemoveIcon.AddManipulator(new Clickable(() => {
                this.model.RemoveComponentByTypeId(this.data.typeId);
            }));

            this.dataFoldout = new Foldout();
            this.dataFoldout.AddToClassList(DATA_FOLDOUT);
            this.dataFoldout.SetValueWithoutNotify(false);

            this.dataContainer = new IMGUIContainer();
            this.dataContainer.cullingEnabled = true;
            this.dataContainer.AddToClassList(DATA_CONTAINER);
            this.dataFoldout.Add(this.dataContainer);

            this.Add(this.tagLabel);
            this.Add(this.labelRemoveIcon);
            this.Add(this.dataFoldout);
            this.Add(this.foldoutRemoveIcon);
        }
        internal void Bind(int typeId) {
            this.data = this.model.GetComponentData(typeId);
            var name = this.data.name;

            if (this.data.isMarker) {
                this.tagLabel.text = $"<color={TAG_COLOR}>{TAG_FORMAT}</color> {name}";
                this.tagLabel.style.display = DisplayStyle.Flex;
                this.dataFoldout.style.display = DisplayStyle.None;
                this.dataContainer.onGUIHandler = null;
            }
            else if(this.data.isNotSerialized) {
                this.tagLabel.text = $"<color={NOT_SERIALIZED_COLOR}>{NOT_SERIALIZED_FORMAT}</color> {name}";
                this.tagLabel.style.display = DisplayStyle.Flex;
                this.dataFoldout.style.display = DisplayStyle.None;
                this.dataContainer.onGUIHandler = null;
            }
            else {
                this.dataFoldout.text = name;
                this.dataFoldout.SetValueWithoutNotify(this.model.IsExpanded(typeId));
                this.dataFoldout.RegisterValueChangedCallback(this.FoldoutExpandedCallback);
                this.dataFoldout.style.display = DisplayStyle.Flex;
                this.tagLabel.style.display = DisplayStyle.None;
                this.dataContainer.onGUIHandler = this.IMGUIHandler;
            }
        }

        internal void Refresh() {
            this.dataFoldout.SetValueWithoutNotify(this.model.IsExpanded(this.data.typeId));
        }

        internal void Reset() {
            this.data = default;
            this.dataContainer.onGUIHandler = null;
            this.dataFoldout.text = string.Empty;
            this.dataFoldout.UnregisterValueChangedCallback(this.FoldoutExpandedCallback);
            this.tagLabel.text = string.Empty;
            this.tagLabel.style.display = DisplayStyle.None;
            this.dataFoldout.style.display = DisplayStyle.Flex;
        }
    }
}
#endif
