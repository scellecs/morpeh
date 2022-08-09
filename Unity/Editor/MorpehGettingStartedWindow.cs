namespace Morpeh.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    internal class MorpehGettingStartedWindow : EditorWindow {
        [InitializeOnLoadMethod]
        public static void ShowIfRequired() {
            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines     = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            if ($";{defines};".Contains(";MORPEH;")) {
                return;
            }

            EditorApplication.delayCall += Open;
        }

        [MenuItem("Tools/Morpeh/Getting Started")]
        public static void Open() {
            var window = GetWindow<MorpehGettingStartedWindow>();
            window.titleContent = new GUIContent("Morpeh");
            window.Show();
        }

        [NonSerialized] private Vector2 scroll;


        private void OnEnable() {
            if (!MorpehModules.CurrentDefines.Contains(MorpehModules.MORPEH_DEFINE)) {
                var definesToAdd = new List<string> {
                    MorpehModules.MORPEH_DEFINE,
                };

                foreach (var moduleInfo in MorpehModules.Modules) {
                    if (moduleInfo.autoEnable) {
                        definesToAdd.AddRange(MorpehModules.EnumerateModuleDefinesRecursive(moduleInfo));
                    }
                }

                MorpehModules.AddDefineSymbols(definesToAdd);
            }
        }

        private void OnGUI() {
            this.DrawHeader();

            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling))
            using (var scrollScope = new GUILayout.ScrollViewScope(this.scroll, false, true)) {
                this.scroll = scrollScope.scrollPosition;

                this.DrawModules();
            }
        }

        private void DrawHeader() {
            using (new GUILayout.VerticalScope(Styles.BigTitle)) {
                GUILayout.Label("Morpeh", Styles.HeaderLabel);
                GUILayout.Label("ECS Framework for Unity Game Engine");
            }
        }

        private void DrawModules() {
            using (new GUILayout.VerticalScope(Styles.BigTitle)) {
                GUILayout.Label("Installed Modules", EditorStyles.largeLabel);

                foreach (var moduleInfo in MorpehModules.Modules) {
                    using (new GUILayout.HorizontalScope(GUI.skin.box)) {
                        GUILayout.Label(moduleInfo.name, EditorStyles.largeLabel);

                        GUILayout.FlexibleSpace();

                        var isModuleEnabled = MorpehModules.IsModuleEnabled(moduleInfo);
                        var canDisable      = MorpehModules.CanDisableModule(moduleInfo, out var disableLockReason);

                        using (new EditorGUI.DisabledScope(!canDisable)) {
                            var toggleContent = new GUIContent("", disableLockReason);

                            if (GUILayout.Toggle(isModuleEnabled, toggleContent) != isModuleEnabled) {
                                if (isModuleEnabled) {
                                    MorpehModules.RemoveDefineSymbols(new[] {moduleInfo.define});
                                }
                                else {
                                    MorpehModules.AddDefineSymbols(MorpehModules.EnumerateModuleDefinesRecursive(moduleInfo));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static class Styles {
            public static readonly GUIStyle HeaderLabel;
            public static readonly GUIStyle BigTitle;

            static Styles() {
                HeaderLabel = new GUIStyle(EditorStyles.largeLabel) {
                    fontSize = 24,
                };
                BigTitle = new GUIStyle("IN BigTitle");
            }
        }
    }
}