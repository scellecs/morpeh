namespace Scellecs.Morpeh.Utils.Editor.Discover {
    using System;
    using UnityEditor;
    using UnityEngine;

    //[CustomEditor(typeof(Discover))]
    public class DiscoverEditor : Editor {
        //private Discover discover;

        public static void DrawDiscoverContentGUI(Discover discover) {
            if (!string.IsNullOrEmpty(discover.Category))
                GUILayout.Label(discover.Category, DiscoverWindow.Styles.subHeader);

            GUILayout.Label(discover.Name, DiscoverWindow.Styles.header);

            using (new GUILayout.VerticalScope(DiscoverWindow.Styles.indent)) {
                if (!string.IsNullOrEmpty(discover.Description)) {
                    GUILayout.Label(discover.Description, DiscoverWindow.Styles.body);
                }

                GUILayout.Space(8);

                if (discover.image != null) {
                    DrawImage(discover.image);
                }

                foreach (var section in discover.Sections) {
                    SectionGUI(section);
                    GUILayout.Space(16);
                }
            }
        }

        public static void DrawImage(Texture texture) {
            float aspect = (float) texture.width / texture.height;
            var   rect   = GUILayoutUtility.GetAspectRect(aspect);
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, false);
        }

        public static void SectionGUI(DiscoverSection section) {
            using (new DiscoverWindow.GroupLabelScope(section.SectionName)) {
                using (new GUILayout.VerticalScope(DiscoverWindow.Styles.slightIndent)) {
                    GUILayout.Label(section.SectionContent, DiscoverWindow.Styles.body);

                    if (section.image != null) {
                        DrawImage(section.image);
                    }

                    if (section.Actions != null && section.Actions.Length > 0) {
                        GUILayout.Space(8);

                        using (new GUILayout.VerticalScope(GUI.skin.box)) {
                            foreach (var action in section.Actions) {
                                using (new GUILayout.HorizontalScope()) {
                                    GUILayout.Label(action.Description);
                                    GUILayout.FlexibleSpace();
                                    using (new GUILayout.HorizontalScope(GUILayout.MinWidth(160),
                                        GUILayout.Height(22))) {
                                        ActionButtonGUI(action.Target);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void ActionButtonGUI(UnityEngine.Object target) {
            if (target == null) {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("(No Object)");
                EditorGUI.EndDisabledGroup();
                return;
            }


            Type t = target.GetType();

            if (t == typeof(GameObject)) {
                GameObject go = target as GameObject;

                if (GUILayout.Button("  Select  ", DiscoverWindow.Styles.buttonLeft)) {
                    Selection.activeObject = go;
                }

                if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab) {
                    if (GUILayout.Button("  Go to  ", DiscoverWindow.Styles.buttonRight)) {
                        Selection.activeObject = go;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
                else {
                    if (GUILayout.Button("  Open  ", DiscoverWindow.Styles.buttonRight)) {
                        AssetDatabase.OpenAsset(go);
                    }
                }
            }
            else if (t == typeof(Discover)) {
                if (GUILayout.Button("Discover")) {
                    var discover = target as Discover;
                    //Selection.activeGameObject = discover;
                    DiscoverWindow.SelectDiscover(discover);
                }
            }
            else if (t == typeof(SceneAsset)) {
                if (GUILayout.Button("Open Scene")) {
                    SceneAsset scene = target as SceneAsset;
                    AssetDatabase.OpenAsset(scene);
                }
            }
            else if (t == typeof(EditorSceneSetup)) {
                if (GUILayout.Button("Open Scenes")) {
                    EditorSceneSetup scene = target as EditorSceneSetup;
                    AssetDatabase.OpenAsset(scene);
                }
            }
            else if (typeof(DiscoverAction).IsAssignableFrom(t)) {
                var action = (DiscoverAction) target;
                if (GUILayout.Button(action.ActionName)) {
                    action.DoAction();
                }
            }
            else {
                if (GUILayout.Button("Select")) {
                    Selection.activeObject = target;
                }
            }
        }
    }
}