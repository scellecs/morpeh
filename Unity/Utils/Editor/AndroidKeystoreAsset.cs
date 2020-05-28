#if UNITY_EDITOR
namespace Morpeh.Unity.Utils.Editor {
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Utils/AndroidKeystore")]
    public class AndroidKeystoreAsset : ScriptableObject {
        public DefaultAsset keyStore;

        public string keyStorePassword;
        public string keyAlias;
        public string keyAliasPassword;

        private void OnValidate() {
            Initialize();
        }

        [InitializeOnLoadMethod]
        public static void Initialize() {
            var androidKeystoreAssets = AssetDatabase.FindAssets("t:AndroidKeystoreAsset");

            if (androidKeystoreAssets.Length > 1) {
                Debug.LogError($"Multiple AndroidKeystoreAssets found. {string.Join(";", androidKeystoreAssets)}");
                return;
            }

            if (androidKeystoreAssets.Length <= 0) {
                return;
            }

            var keystoreAssetGuid = androidKeystoreAssets[0];
            var keystoreAssetPath = AssetDatabase.GUIDToAssetPath(keystoreAssetGuid);
            var keystoreAsset     = AssetDatabase.LoadAssetAtPath<AndroidKeystoreAsset>(keystoreAssetPath);
            var keystorePath      = AssetDatabase.GetAssetPath(keystoreAsset.keyStore);

            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystoreAsset.keyStorePassword;
            PlayerSettings.Android.keyaliasName = keystoreAsset.keyAlias;
            PlayerSettings.Android.keyaliasPass = keystoreAsset.keyAliasPassword;
        }
    }
}
#endif