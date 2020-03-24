namespace Morpeh.Utils.Editor {
    using Utils.Editor;
    using UnityEditor;

    static class DiscoverMorpeh {
        [MenuItem("Help/Discover Morpeh", priority = 0)]
        static void ShowSpaceshipDiscover() {
            var asset = AssetDatabase.LoadAssetAtPath<DiscoverAsset>("Assets/Morpeh/Unity/Utils/Editor/Discover/DiscoverMorpeh.asset");
            DiscoverWindow.ShowDiscoverWindow(asset);
        }
    }
}