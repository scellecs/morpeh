namespace Scellecs.Morpeh.Utils.Editor.Discover {
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    public class DiscoverAsset : ScriptableObject {
        [MenuItem("Help/Morpeh Discover", priority = 0)]
        internal static void Show() {
            var asset = AssetDatabase.LoadAssetAtPath<DiscoverAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:DiscoverAsset")[0]));
            DiscoverWindow.ShowDiscoverWindow(asset);
        }
        
        [OnOpenAsset]
        static bool OpenAsset(int instanceID, int line) {
            var asset = EditorUtility.InstanceIDToObject(instanceID);
            if (asset is DiscoverAsset) {
                DiscoverWindow.ShowDiscoverWindow(asset as DiscoverAsset);
                return true;
            }
            else
                return false;
        }

        [Header("General Properties")]
        public string WindowTitle = "Discover";
        public Texture2D HeaderTexture;
        public bool      dockable = false;

        [Tooltip("Width of the Window, in pixels")]
        public int WindowWidth = 640;

        [Tooltip("Height of the Window, in pixels")]
        public int WindowHeight = 520;

        [Tooltip("Width of the Discover List, in pixels")]
        public int DiscoverListWidth = 180;

        [Header("Show At Startup")]
        public bool EnableShowAtStartup = true;

        [Tooltip("The name of the preference for auto showing at startup, will be ")]
        public string PreferenceName = "Discover";

        [Header("Debug")]
        public bool Debug = false;
    }
}
