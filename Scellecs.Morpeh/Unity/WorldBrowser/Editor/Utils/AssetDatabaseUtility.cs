#if UNITY_EDITOR
using UnityEditor;
namespace Scellecs.Morpeh.WorldBrowser.Editor.Utils {
    internal static class AssetDatabaseUtility {
        internal static T LoadAssetWithGUID<T>(string guid) where T : UnityEngine.Object {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
#endif