namespace Morpeh.Utils.Editor {
    using UnityEditor;

    internal static class DiscoverMorpeh {
        [MenuItem("Help/Discover Morpeh", priority = 0)]
        internal static void Show() {
            var asset = AssetDatabase.LoadAssetAtPath<DiscoverAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DiscoverMorpeh")[0]));
            DiscoverWindow.ShowDiscoverWindow(asset);
        }
    }
}