#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
namespace Scellecs.Morpeh.Editor {
    using System.Threading.Tasks;
    using UnityEditor;
	using UnityEditor.Build;
    using UnityEditor.PackageManager;

    [InitializeOnLoad]
    internal static class BurstDetector {
        private const string DEFINITION_NAME = "MORPEH_BURST";

        static BurstDetector() {
            ResolveDependencies();
        }
        private static async void ResolveDependencies() {
            var packages      = Client.List(true, true);
            while (packages.Status == StatusCode.InProgress) {
                await Task.Yield();
            }

            if (packages.Status != StatusCode.Success) return;

            foreach (var package in packages.Result) {
                if (package.name == "com.unity.burst") {
                    SetDefine(DEFINITION_NAME);
                    return;
                }
            }
            RemoveDefine(DEFINITION_NAME);
        }

        private static BuildTargetGroup GetActiveBuildTargetGroup() {
            return BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        }

        private static string GetDefinesString() {
            var buildTargetGroup = GetActiveBuildTargetGroup();
#if UNITY_6000_0_OR_NEWER
            return PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif
        }

        private static void SetDefine(string newDefine) {
            if (!IsDefined(newDefine)) {
                var replacement      = GetDefinesString() + ";" + newDefine;
                var buildTargetGroup = GetActiveBuildTargetGroup();
#if UNITY_6000_0_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), replacement);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, replacement);
#endif
            }
        }

        private static void RemoveDefine(string def) {
            if (IsDefined(def)) {
                var currentDefs = GetDefinesString().Split(';');
                var newDefs     = "";

                foreach (var t in currentDefs) {
                    if (t != def) newDefs += t + ";";
                }

                var buildTargetGroup = GetActiveBuildTargetGroup();
#if UNITY_6000_0_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), newDefs);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefs);
#endif
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