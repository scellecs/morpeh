#if UNITY_EDITOR
namespace Morpeh.Utils.Editor {
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    public static class OpenAssetProcessor {
        [OnOpenAsset(-1000)]
        public static bool OnOpenAsset(int instanceID, int line) {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is ScriptableObject so) {
                if (so is IInitializer && so is Configurator == false) {
                    var monoScript = MonoScript.FromScriptableObject(so);
                    AssetDatabase.OpenAsset(monoScript);
                    return true;
                }
            }
            return false;
        }
    }
}
#endif