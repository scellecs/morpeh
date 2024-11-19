#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;
namespace Scellecs.Morpeh.Utils.Editor {
    [HideMonoScript]
    internal sealed class ComponentViewWrapper : MonoBehaviour {
        [HideLabel]
        [InlineProperty]
        [ShowInInspector]
        internal ComponentData component;
    }
}
#endif