using System;
using System.Collections.Generic;

#if UNITY_EDITOR && ODIN_INSPECTOR
namespace Morpeh.Unity.Utils.Editor {
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    internal class EditorReference : IDisposable
    {
        private GameObject gameObject;
        private Editor editor;
        
        public Vector2 ScrollPos;

        public EditorReference(GameObject gameObject, Editor editor)
        {
            this.gameObject = gameObject;
            this.editor = editor;
        }

        public void HandleInspectorGUI()
        {
            editor.OnInspectorGUI();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(gameObject);
            gameObject = null;
            
            Object.DestroyImmediate(editor);
            editor = null;
        }
    }

    public class WorldBrowser : EditorWindow {
        private List<EditorReference> references;
        
        [MenuItem("Tools/Morpeh/World Browser")]
        private static void OpenWindow() => GetWindow<WorldBrowser>().Show();

        private void EditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredPlayMode) {
                Initialize();
            }

            if (state == PlayModeStateChange.ExitingPlayMode) {
                Flush();
            }
        }

        private void Initialize()
        {
            references = new List<EditorReference>();
            
            foreach (var world in World.worlds)
            {
                var gameObject = new GameObject("MORPEH__WORLD_VIEWER") {hideFlags = HideFlags.HideAndDontSave};
                DontDestroyOnLoad(gameObject);
                
                var viewer = gameObject.AddComponent<WorldViewer>();
                viewer.Ctor(world);
                
                var editor = Editor.CreateEditor(viewer);
                
                references.Add(new EditorReference(gameObject, editor));
            }
        }

        private void Flush() {
            foreach (var reference in references)
            {
                reference.Dispose();
            }

            references = null;
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
            if (references == null) return;
            
            foreach (var reference in references)
            {
                reference.ScrollPos = EditorGUILayout.BeginScrollView(reference.ScrollPos);
                GUIHelper.PushHierarchyMode(false);
                reference.HandleInspectorGUI();
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
