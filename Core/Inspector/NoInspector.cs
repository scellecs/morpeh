#if !ODIN_INSPECTOR && !TRI_INSPECTOR
namespace Sirenix.OdinInspector {
    using System;

    internal class ShowInInspectorAttribute : Attribute {
    }

    internal class RequiredAttribute : Attribute {
    }

    internal class InfoBoxAttribute : Attribute {
        public InfoBoxAttribute(string message, InfoMessageType messageType, string visibleIf) {
        }
    }

    internal class PropertyOrder : Attribute {
        public PropertyOrder(int order) {
        }
    }

    internal class OnValueChangedAttribute : Attribute {
        public OnValueChangedAttribute(string method) {
        }
    }

    internal class HorizontalGroupAttribute : Attribute {
        public HorizontalGroupAttribute(string name, float width = 0f) {
        }
    }

    internal class HideLabelAttribute : Attribute {
    }

    internal class HideMonoScriptAttribute : Attribute {
    }

    internal class OnInspectorGUIAttribute : Attribute {
    }

    internal class ReadOnlyAttribute : Attribute {
    }

    internal class HideIfAttribute : Attribute {
        public HideIfAttribute(string expr) {
        }
    }

    internal class ShowIfAttribute : Attribute {
        public ShowIfAttribute(string expr) {
        }
    }

    internal class PropertySpaceAttribute : Attribute {
    }

    internal class InlinePropertyAttribute : Attribute {
    }

    internal class DisableContextMenuAttribute : Attribute {
    }

    internal class HideReferenceObjectPickerAttribute : Attribute {
    }

    internal class ListDrawerSettingsAttribute : Attribute {
        public bool DraggableItems;
        public bool HideAddButton;
        public bool HideRemoveButton;
    }

    internal class TitleAttribute : Attribute {
        public TitleAttribute(string title, string subtitle = null) {
        }

        public bool HorizontalLine;
    }

    internal class PropertyTooltipAttribute : Attribute {
        public PropertyTooltipAttribute(string tooltip) {
        }
    }

    internal class DisplayAsStringAttribute : Attribute {
        public DisplayAsStringAttribute(bool overflow) {
        }
    }

    internal class SearchableAttribute : Attribute {
    }

    internal class LabelTextAttribute : Attribute {
        public LabelTextAttribute(string text) {
        }
    }

    internal class ButtonAttribute : Attribute {
        public string Name;
    }

    internal enum InfoMessageType {
        Error,
    }
}
#endif