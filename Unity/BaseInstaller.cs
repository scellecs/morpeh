namespace Morpeh {
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#elif UNITY_EDITOR && TRI_INSPECTOR
    // TODO: TRI_INSPECTOR SUPPORT
#endif
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

#if UNITY_EDITOR && ODIN_INSPECTOR
    [HideMonoScript]
#elif UNITY_EDITOR && TRI_INSPECTOR
    // TODO: TRI_INSPECTOR SUPPORT
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class BaseInstaller : WorldViewer {
        protected abstract void OnEnable();

        protected abstract void OnDisable();
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnInspectorGUI]
        private void OnEditorGUI() {
            this.gameObject.transform.hideFlags = HideFlags.HideInInspector;
        }
#elif UNITY_EDITOR && TRI_INSPECTOR
        // TODO: TRI_INSPECTOR SUPPORT
#endif
#if UNITY_EDITOR
        [MenuItem("GameObject/ECS/", true, 10)]
        private static bool OrderECS() => true;
#endif
    }
}
