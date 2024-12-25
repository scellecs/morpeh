#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer {
    [HideMonoScript]
    internal sealed class ComponentViewWrapper : ScriptableObject {
        [HideLabel]
        [InlineProperty]
        [ShowInInspector]
        internal ComponentData component;
    }
}
#endif