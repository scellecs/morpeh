#if UNITY_EDITOR
namespace Scellecs.Morpeh.Utils.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.OdinInspector;

    [CreateAssetMenu(menuName = "ECS/Utils/Define")]
    public class DefineAsset : ScriptableObject {
        private const string PREFS_KEY = "__MORPEH_DEFINES";
        [SerializeField]
        private List<DefineWrapper> defines = default;

        [Button]
        [PropertyOrder(-10)]
        public void Apply() {
            Initialize();
        }

        [Serializable, InlineProperty]
        private class DefineWrapper {
            [HideLabel]
            public string define = default;

            public override string ToString() => this.define;
        }

        public class DefineAssetModificationProcessor : AssetPostprocessor {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
                EditorApplication.delayCall += Initialize;
            }
        }

        [InitializeOnLoadMethod]
        private static void Initialize() {
            var buildGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            var cachedDefines = new List<string>();
            if (EditorPrefs.HasKey(PREFS_KEY)) {
                cachedDefines = EditorPrefs.GetString(PREFS_KEY).Split(',').ToList();
            }

            var addedDefines  = new List<string>();
            var existsDefines = new List<string>();
            var savedDefines  = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);
            if (!savedDefines.EndsWith(";")) {
                savedDefines += ";";
            }

            var guids = AssetDatabase.FindAssets("t:DefineAsset");
            foreach (var guid in guids) {
                if (string.IsNullOrEmpty(guid)) {
                    continue;
                }

                var defineAsset = AssetDatabase.LoadAssetAtPath<DefineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (defineAsset != null) {
                    foreach (var define in defineAsset.defines) {
                        if (!savedDefines.Contains($";{define};") && !savedDefines.StartsWith($"{define};")) {
                            savedDefines += $"{define};";
                            addedDefines.Add(define.define);
                        }
                        else {
                            existsDefines.Add(define.define);
                        }
                    }
                }
            }

            var summaryDefines = addedDefines.Concat(existsDefines).ToList();

            cachedDefines.RemoveAll(summaryDefines.Contains);
            foreach (var cachedDefine in cachedDefines) {
                if (string.IsNullOrEmpty(cachedDefine)) {
                    continue;
                }

                savedDefines = savedDefines.Replace($"{cachedDefine};", string.Empty);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, savedDefines);
            EditorPrefs.SetString(PREFS_KEY, string.Join(",", summaryDefines));
        }
    }
}
#endif