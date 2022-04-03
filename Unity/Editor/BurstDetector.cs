#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
namespace Morpeh.Editor {
    using System;
    using UnityEditor;

    [InitializeOnLoad]
    internal static class BurstDetector {
        private const string DEFINITION_NAME = "MORPEH_BURST";

        static BurstDetector() {
            var hasBurst = CountTypesInNamespace("Unity.Burst") > 100;
            var hasJobs  = CountTypesInNamespace("Unity.Jobs") > 20;
            if (hasBurst && hasJobs) {
                SetDefine(DEFINITION_NAME);
            }
            else {
                RemoveDefine(DEFINITION_NAME);
            }
        }

        private static int CountTypesInNamespace(string nameSpace, string root = "Assembly") {
            var childTypesCount = 0;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                try {
                    var types = assembly.GetTypes();
                    foreach (var type in types) {
                        if (type.Namespace != null && type.Namespace.StartsWith(nameSpace)) {
                            childTypesCount++;
                        }
                    }
                }
                catch (Exception) {
                    // Skip
                }
            }

            return childTypesCount;
        }

        private static string GetDefinesString() => PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        private static void SetDefine(string newDefine) {
            if (!IsDefined(newDefine)) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, GetDefinesString() + ";" + newDefine);
            }
        }

        private static void RemoveDefine(string def) {
            if (IsDefined(def)) {
                var currentDefs = GetDefinesString().Split(';');
                var newDefs     = "";

                foreach (var t in currentDefs) {
                    if (t != def) newDefs += t + ";";
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefs);
            }
        }

        private static bool IsDefined(string def) {
            var currentSymbols = GetDefinesString().Split(';');
            foreach (var t in currentSymbols) {
                if (t == def) return true;
            }

            return false;
        }
    }
}
#endif