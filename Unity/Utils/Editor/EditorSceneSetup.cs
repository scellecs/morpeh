namespace Scellecs.Morpeh.Utils.Editor {
    using System;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public class EditorSceneSetup : ScriptableObject {
        [MenuItem("File/Save Scene Setup As... #%&S", priority = 171)]
        private static void SaveSetup() {
            var path = EditorUtility.SaveFilePanelInProject("Save EditorSceneSetup", "New EditorSceneSetup", "asset",
                "Save EditorSceneSetup?");
            if (path != string.Empty) {
                var setup = GetCurrentSetup();
                AssetDatabase.CreateAsset(setup, path);
            }
        }

        public delegate void EditorSceneSetupLoadedDelegate(EditorSceneSetup setup);

        public static event EditorSceneSetupLoadedDelegate onSetupLoaded;
        
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line) {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is EditorSceneSetup) {
                var setup = (EditorSceneSetup)obj;
                var active = setup.ActiveScene;

                try {
                    EditorUtility.DisplayProgressBar("Loading Scenes",
                        string.Format("Loading Scene Setup {0}....", setup.name), 1.0f);
                    RestoreSetup(setup);
                }
                finally {
                    EditorUtility.ClearProgressBar();
                }

                return true;
            }

            return false;
        }

        [MenuItem("Assets/Create/Editor Scene Setup", priority = 200)]
        private static void CreateAsset() {
            CreateAssetInProjectWindow<EditorSceneSetup>("SceneSet Icon", "New SceneSetup.asset");
        }

        private static void CreateAssetInProjectWindow<T>(string iconName, string fileName) where T : ScriptableObject {
            var icon = EditorGUIUtility.FindTexture(iconName);

            var namingInstance = new DoCreateGenericAsset();
            namingInstance.type = typeof(T);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, namingInstance, fileName, icon, null);
        }
        
        private static ScriptableObject CreateAssetAtPath(string path, Type type) {
            var asset = ScriptableObject.CreateInstance(type);
            asset.name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
        
        private class DoCreateGenericAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction {
            public Type type;

            public override void Action(int instanceId, string pathName, string resourceFile) {
                ScriptableObject asset = CreateAssetAtPath(pathName, this.type);
                ProjectWindowUtil.ShowCreatedAsset(asset);
            }
        }

        public int ActiveScene;
        public EditorScene[] LoadedScenes;

        [Serializable]
        public struct EditorScene {
            public SceneAsset Scene;
            public bool Loaded;
        }

        public static EditorSceneSetup GetCurrentSetup() {
            var scenesetups = EditorSceneManager.GetSceneManagerSetup();

            var editorSetup = CreateInstance<EditorSceneSetup>();

            var i = 0;
            editorSetup.LoadedScenes = new EditorScene[scenesetups.Length];
            foreach (var setup in scenesetups) {
                if (setup.isActive) {
                    editorSetup.ActiveScene = i;
                }

                editorSetup.LoadedScenes[i].Scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(setup.path);
                editorSetup.LoadedScenes[i].Loaded = setup.isLoaded;

                i++;
            }

            return editorSetup;
        }

        public static void RestoreSetup(EditorSceneSetup editorSetup) {
            var setups = new SceneSetup[editorSetup.LoadedScenes.Length];

            for (var i = 0; i < setups.Length; i++) {
                setups[i] = new SceneSetup();
                var path = AssetDatabase.GetAssetPath(editorSetup.LoadedScenes[i].Scene);
                setups[i].path = path;
                setups[i].isLoaded = editorSetup.LoadedScenes[i].Loaded;
                setups[i].isActive = editorSetup.ActiveScene == i;
            }

            EditorSceneManager.RestoreSceneManagerSetup(setups);

            if (onSetupLoaded != null) {
                onSetupLoaded.Invoke(editorSetup);
            }
        }
    }
}
