namespace Morpeh.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Callbacks;

    internal static class MorpehModules {
        public const string MORPEH_DEFINE = "MORPEH";

        public static BuildTargetGroup CurrentTargetGroup;
        public static string[]         CurrentDefines;

        [InitializeOnLoadMethod]
        [DidReloadScripts]
        public static void Refresh() {
            CurrentTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            CurrentDefines     = PlayerSettings.GetScriptingDefineSymbolsForGroup(CurrentTargetGroup).Split(';');
        }

        public static readonly MorpehModuleInfo[] Modules = {
            new MorpehModuleInfo {
                name       = "Morpeh.Globals",
                define     = "MORPEH_GLOBALS",
                autoEnable = true,
            },
            new MorpehModuleInfo {
                name       = "Morpeh.Systems",
                define     = "MORPEH_SYSTEMS",
                autoEnable = true,
            },
            new MorpehModuleInfo {
                name         = "Morpeh.Installers",
                define       = "MORPEH_INSTALLERS",
                dependencies = new[] {"Morpeh.Systems"},
                autoEnable   = true,
            },
            new MorpehModuleInfo {
                name       = "Morpeh.Providers",
                define     = "MORPEH_PROVIDERS",
                autoEnable = true,
            },
            new MorpehModuleInfo {
                name         = "Morpeh.Utils",
                define       = "MORPEH_UTILS",
                dependencies = new[] {"Morpeh.Globals", "Morpeh.Systems", "Morpeh.Installers", "Morpeh.Providers"},
            },
        };

        public static bool IsModuleEnabled(MorpehModuleInfo moduleInfo) {
            return CurrentDefines.Contains(moduleInfo.define);
        }

        public static bool CanDisableModule(MorpehModuleInfo moduleInfo, out string reason) {
            reason = null;

            foreach (var other in Modules) {
                if (IsModuleEnabled(other) && other.dependencies.Contains(moduleInfo.name)) {
                    if (reason != null) {
                        reason += Environment.NewLine;
                    }

                    reason += $"Required by {other.name}";
                }
            }

            return reason == null;
        }

        public static MorpehModuleInfo FindModule(string moduleName) {
            foreach (var moduleInfo in Modules) {
                if (moduleInfo.name == moduleName) {
                    return moduleInfo;
                }
            }

            return null;
        }

        public static IEnumerable<string> EnumerateModuleDefinesRecursive(MorpehModuleInfo moduleInfo) {
            yield return moduleInfo.define;

            foreach (var dependencyModuleName in moduleInfo.dependencies) {
                var dependencyModule = FindModule(dependencyModuleName);
                foreach (var define in EnumerateModuleDefinesRecursive(dependencyModule)) {
                    yield return define;
                }
            }
        }

        public static void AddDefineSymbols(IEnumerable<string> definesToAdd) {
            var newDefines = new List<string>(CurrentDefines);

            foreach (var define in definesToAdd) {
                if (!newDefines.Contains(define)) {
                    newDefines.Add(define);
                }
            }

            var newDefinesString = string.Join(";", newDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(CurrentTargetGroup, newDefinesString);
        }

        public static void RemoveDefineSymbols(IEnumerable<string> definesToRemove) {
            var newDefines = new List<string>(CurrentDefines);

            foreach (var define in definesToRemove) {
                newDefines.Remove(define);
            }

            var newDefinesString = string.Join(";", newDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(CurrentTargetGroup, newDefinesString);
        }

        [Serializable]
        public class MorpehModuleInfo {
            public string   name;
            public string   define;
            public string[] dependencies = Array.Empty<string>();
            public bool     autoEnable;
        }
    }
}