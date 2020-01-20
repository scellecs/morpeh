#if UNITY_EDITOR && ODIN_INSPECTOR
namespace Morpeh.Unity.Utils.Editor {
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public class WorldBrowser : EditorWindow {
        private Editor editor;
        private Vector2 scrollPos;
        private GameObject gameObject;
        
        [MenuItem("Tools/Morpeh/World Browser")]
        private static void OpenWindow() => GetWindow<WorldBrowser>().Show();

        private void EditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredPlayMode) {
                this.Initialize();
            }

            if (state == PlayModeStateChange.ExitingPlayMode) {
                this.Flush();
            }
        }

        private void Initialize() {
            this.gameObject = new GameObject("MORPEH__WORLD_VIEWER") {hideFlags = HideFlags.HideAndDontSave};
            var viewer = this.gameObject.AddComponent<WorldViewer>();
            this.editor = Editor.CreateEditor(viewer);
        }

        private void Flush() {
            DestroyImmediate(this.gameObject);
            DestroyImmediate(this.editor);
        }
        
        private void OnEnable() {
            titleContent = new GUIContent("World Browser",
                Resources.Load<Texture>("MorpehIcons/64x64_W"),
                "Entities Browser");
            
            EditorApplication.playModeStateChanged += this.EditorApplicationOnplayModeStateChanged;
            if (EditorApplication.isPlaying) {
                this.Initialize();
            }
        }

        private void OnGUI()
        {
            if (this.editor != null) {
                this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
                GUIHelper.PushHierarchyMode(false);
                this.editor.OnInspectorGUI();
                GUIHelper.PopHierarchyMode();
                EditorGUILayout.EndScrollView();

                this.RepaintIfRequested();
            }
        }
 
        private void OnDestroy() {
            EditorApplication.playModeStateChanged -= this.EditorApplicationOnplayModeStateChanged;
            this.Flush();
        }
    }
}
#endif
