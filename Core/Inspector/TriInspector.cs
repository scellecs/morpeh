#if !ODIN_INSPECTOR && TRI_INSPECTOR
using TriInspector;

namespace Sirenix.OdinInspector {
    using System;

    internal class ShowInInspectorAttribute : TriInspector.ShowInInspectorAttribute {
    }

    internal class RequiredAttribute : TriInspector.RequiredAttribute {
    }

    internal class InfoBoxAttribute : TriInspector.InfoBoxAttribute
    {
        public InfoBoxAttribute(string message, InfoMessageType messageType, string visibleIf)
            : base(text: message, messageType: GetTriMessageType(messageType), visibleIf: visibleIf) {
        }

        private static TriInspector.TriMessageType GetTriMessageType(InfoMessageType type) {
            return TriMessageType.Error;
        }
    }

    internal class PropertyOrderAttribute : TriInspector.PropertyOrderAttribute {
        public PropertyOrderAttribute(int order) : base(order) {
        }
    }

    internal class OnValueChangedAttribute : TriInspector.OnValueChangedAttribute {
        public OnValueChangedAttribute(string method) : base(method) {
        }
    }

    internal class HorizontalGroupAttribute : TriInspector.GroupAttribute {
        public HorizontalGroupAttribute(string name, float width = 0f) : base(name) {
        }
    }

    internal class HideLabelAttribute : TriInspector.HideLabelAttribute {
    }

    internal class HideMonoScriptAttribute : TriInspector.HideMonoScriptAttribute {
    }

    internal class OnInspectorGUIAttribute : Attribute {
    }

    internal class ReadOnlyAttribute : TriInspector.ReadOnlyAttribute {
    }

    internal class HideIfAttribute : TriInspector.HideIfAttribute {
        public HideIfAttribute(string expr) : base(expr) {
        }
    }

    internal class ShowIfAttribute : TriInspector.ShowIfAttribute {
        public ShowIfAttribute(string expr) : base(expr) {
        }
    }

    internal class PropertySpaceAttribute : TriInspector.PropertySpaceAttribute {
    }

    internal class InlinePropertyAttribute : TriInspector.InlinePropertyAttribute {
    }

    internal class DisableContextMenuAttribute : Attribute {
    }

    internal class HideReferenceObjectPickerAttribute : TriInspector.HideReferencePickerAttribute {
    }

    internal class ListDrawerSettingsAttribute : TriInspector.ListDrawerSettingsAttribute {
        public bool DraggableItems
        {
            get => base.Draggable;
            set => base.Draggable = value;
        }
    }

    internal class TitleAttribute : TriInspector.TitleAttribute {
        public TitleAttribute(string title, string subtitle = null) 
            : base(string.IsNullOrEmpty(title) ? subtitle : title) {
        }
    }

    internal class PropertyTooltipAttribute : TriInspector.PropertyTooltipAttribute {
        public PropertyTooltipAttribute(string tooltip) : base(tooltip) {
        }
    }
    
    internal class DisplayAsStringAttribute : TriInspector.DisplayAsStringAttribute {
        public DisplayAsStringAttribute(bool overflow) {
        }
    }

    internal class SearchableAttribute : Attribute {
    }

    internal class LabelTextAttribute : TriInspector.LabelTextAttribute {
        public LabelTextAttribute(string text) : base(text) {
        }
    }

    internal class ButtonAttribute : TriInspector.ButtonAttribute {
    }

    internal enum InfoMessageType {
        Error,
    }
}
#endif