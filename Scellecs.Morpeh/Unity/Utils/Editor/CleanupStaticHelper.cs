#if UNITY_EDITOR
using Scellecs.Morpeh.Collections;
using Scellecs.Morpeh.Providers;
using UnityEditor;
using UnityEngine;

namespace Scellecs.Morpeh.Utils.Editor {
    internal static class CleanupStaticHelper {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubscribeCleanup() {
            EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
        }

        private static void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredEditMode) {
                World.CleanupStatic();
                EntityProvider.map.Clear();
                EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
            }
        }
    }
}
#endif