#if !MORPEH_DISABLE_SET_ICONS
namespace Scellecs.Morpeh.Utils.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Scellecs.Morpeh;
    using Scellecs.Morpeh.Providers;
    using Scellecs.Morpeh.Systems;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal class IconsSetter : AssetPostprocessor {
        private static Action<MonoImporter, Texture2D> SelectAndSaveIcon;

        //TODO refactor to dictionary
        private static Texture2D iconC;
        private static Texture2D iconE;
        private static Texture2D iconF;
        private static Texture2D iconG;
        private static Texture2D iconI;
        private static Texture2D iconL;
        private static Texture2D iconM;
        private static Texture2D iconP;
        private static Texture2D iconS;
        private static Texture2D iconT;
        private static Texture2D iconU;
        private static Texture2D iconV;
        private static Texture2D iconW;

        [InitializeOnLoadMethod]
        private static void Initialize() {
#if UNITY_2021_3_OR_NEWER
            SelectAndSaveIcon = (importer, icon) => {
                importer.SetIcon(icon);
                importer.SaveAndReimport();
            };
#else
            var method  = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var method2 = typeof(MonoImporter).GetMethod("CopyMonoScriptIconToImporters", BindingFlags.Static | BindingFlags.NonPublic);
            var selectIcon = (Action<Object, Texture2D>) Delegate.CreateDelegate(typeof(Action<Object, Texture2D>), method);
            var saveIcon   = (Action<MonoScript>) Delegate.CreateDelegate(typeof(Action<MonoScript>), method2);
            
            SelectAndSaveIcon = (importer, icon) => {
                var script = importer.GetScript();
                selectIcon(script, icon);
                saveIcon(script);
            };
#endif

            iconC = Resources.Load<Texture2D>("MorpehIcons/64x64_C");
            iconE = Resources.Load<Texture2D>("MorpehIcons/64x64_E");
            iconF = Resources.Load<Texture2D>("MorpehIcons/64x64_F");
            iconG = Resources.Load<Texture2D>("MorpehIcons/64x64_G");
            iconI = Resources.Load<Texture2D>("MorpehIcons/64x64_I");
            iconL = Resources.Load<Texture2D>("MorpehIcons/64x64_L");
            iconM = Resources.Load<Texture2D>("MorpehIcons/64x64_M");
            iconP = Resources.Load<Texture2D>("MorpehIcons/64x64_P");
            iconS = Resources.Load<Texture2D>("MorpehIcons/64x64_S");
            iconT = Resources.Load<Texture2D>("MorpehIcons/64x64_T");
            iconU = Resources.Load<Texture2D>("MorpehIcons/64x64_U");
            iconV = Resources.Load<Texture2D>("MorpehIcons/64x64_V");
            iconW = Resources.Load<Texture2D>("MorpehIcons/64x64_W");
        }

        private const string KEY = "MORPEH__SCRIPTS_FOR_SETTING_ICONS";

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            if (EditorPrefs.HasKey(KEY)) {
                var descriptor = JsonUtility.FromJson<ScriptsForIcons>(EditorPrefs.GetString(KEY));
                foreach (var path in descriptor.Scripts) {
                    var monoImporter = MonoImporter.GetAtPath(path) as MonoImporter;
                    if (monoImporter != null) {
                        var script           = monoImporter.GetScript();
                        var type = script.GetClass();
                        if (type == null) {
                            continue;
                        }
                        
                        var serializedObject = new SerializedObject(script);
                        var iconProperty     = serializedObject.FindProperty("m_Icon");
                        if (iconProperty.objectReferenceValue != null) {
                            continue;
                        }
                        
                        if (InheritsFrom(type, typeof(Initializer))) {
                            SelectAndSaveIcon(monoImporter, iconI);
                        }
                        else if (InheritsFrom(type, typeof(MonoProvider<>))) {
                            SelectAndSaveIcon(monoImporter, iconP);
                        }
                        else if (InheritsFrom(type, typeof(IComponent))) {
                            SelectAndSaveIcon(monoImporter, iconC);
                        }
                        else if (InheritsFrom(type, typeof(IFixedSystem))) {
                            SelectAndSaveIcon(monoImporter, iconF);
                        }
                        else if (InheritsFrom(type, typeof(ILateSystem))) {
                            SelectAndSaveIcon(monoImporter, iconL);
                        }
                        else if (InheritsFrom(type, typeof(ICleanupSystem))) {
                            SelectAndSaveIcon(monoImporter, iconC);
                        }
                        else if (InheritsFrom(type, typeof(ISystem))) {
                            SelectAndSaveIcon(monoImporter, iconU);
                        }
                        // else if (InheritsFrom(type, typeof(BaseGlobalVariable<>))) {
                        //     SelectAndSaveIcon(monoImporter, iconV);
                        // }
                        // else if (InheritsFrom(type, typeof(BaseGlobalEvent<>))) {
                        //     SelectAndSaveIcon(monoImporter, iconE);
                        // }
                    }
                }

                EditorPrefs.DeleteKey(KEY);
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            var descriptor = new ScriptsForIcons {Scripts = new List<string>()};
            foreach (var path in importedAssets) {
                var monoImporter = MonoImporter.GetAtPath(path) as MonoImporter;
                if (monoImporter != null) {
                    descriptor.Scripts.Add(path);
                }
            }

            EditorPrefs.SetString(KEY, JsonUtility.ToJson(descriptor));
        }
        
        private static bool InheritsFrom(System.Type type, System.Type baseType)
        {
            if (baseType.IsAssignableFrom(type))
                return true;
            if (type.IsInterface && !baseType.IsInterface)
                return false;
            if (baseType.IsInterface)
                return ((IEnumerable<System.Type>) type.GetInterfaces()).Contains<System.Type>(baseType);
            for (System.Type type1 = type; type1 != null; type1 = type1.BaseType)
            {
                if (type1 == baseType || baseType.IsGenericTypeDefinition && type1.IsGenericType && type1.GetGenericTypeDefinition() == baseType)
                    return true;
            }
            return false;
        }

        [Serializable]
        private struct ScriptsForIcons {
            public List<string> Scripts;
        }
    }
}
#endif