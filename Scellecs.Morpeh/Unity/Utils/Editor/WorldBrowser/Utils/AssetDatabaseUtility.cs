using UnityEditor;

namespace Scellecs.Morpeh.Utils.Editor {
    internal static class AssetDatabaseUtility {
        internal static T LoadAssetWithGUID<T>(string guid) where T : UnityEngine.Object {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}