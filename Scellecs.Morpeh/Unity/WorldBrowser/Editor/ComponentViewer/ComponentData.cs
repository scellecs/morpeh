#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;

namespace Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer {
    [Serializable]
    internal struct ComponentData {
        internal int typeId;
        internal string name;
        internal bool isMarker;
        internal bool isNotSerialized;

        internal Func<int, object> Get;
        internal Action<int, object> Set;

        [HideLabel]
        [InlineProperty]
        [ShowInInspector]
        [DisableContextMenu]
        [HideReferenceObjectPicker]
        public object Data {
            get => Get?.Invoke(this.typeId);
            set => Set?.Invoke(this.typeId, value);
        }
    }
}
#endif
