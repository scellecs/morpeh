#if UNITY_EDITOR
namespace Morpeh.Apple {
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Linq;
#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif
    
    [CreateAssetMenu(menuName = "ECS/Utils/iOSPostBuildProcessor")]
    public class iOSPostBuildProcessor : ScriptableObject {
        public int appID;

        public List<iOSFrameworkDescription> frameworks;

        public List<BuildProperties> flags;
        public PlistKeys plistKeys;
        public DefaultAsset copyFilesDirectory;
        public DefaultAsset entitlementsFile;
        public DefaultAsset newDelegateFile;
        
#if UNITY_IOS
        [UnityEditor.Callbacks.PostProcessBuild(999)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {
            var guids = AssetDatabase.FindAssets("t:iOSPostBuildProcessor");
            var settings = new PostBuildProcessingSettings();

            foreach (var guid in guids) {
                if (string.IsNullOrEmpty(guid)) continue;
                var config = AssetDatabase.LoadAssetAtPath<iOSPostBuildProcessor>(AssetDatabase.GUIDToAssetPath(guid));
                if (config == null) continue;
                settings.AddSettings(config);
            }
            
            Tools.Process(settings, path);
        }
#endif
    }

#if UNITY_IOS    
    class PostBuildProcessingSettings {
        public List<iOSFrameworkDescription> frameworks = new List<iOSFrameworkDescription>();
        public List<BuildProperties> flags = new List<BuildProperties>();
        public PlistKeys plistKeys = new PlistKeys();
        public List<DefaultAsset> copyFilesDirectories = new List<DefaultAsset>();
        public List<DefaultAsset> entitlementsFiles = new List<DefaultAsset>();
        public List<DefaultAsset> newDelegateFiles = new List<DefaultAsset>();

        public void AddSettings(iOSPostBuildProcessor config) {
            if(config.copyFilesDirectory != null && !this.copyFilesDirectories.Contains(config.copyFilesDirectory)) 
                this.copyFilesDirectories.Add(config.copyFilesDirectory);
            
            if(config.entitlementsFile != null && !this.entitlementsFiles.Contains(config.entitlementsFile)) 
                this.entitlementsFiles.Add(config.entitlementsFile);
            
            if(config.newDelegateFile != null && !this.newDelegateFiles.Contains(config.newDelegateFile)) 
                this.newDelegateFiles.Add(config.newDelegateFile);

            foreach (var boolKey in config.plistKeys.BoolKeys)
            {
                this.AddPlistBool(boolKey);
            }
            foreach (var boolKey in config.plistKeys.StringKeys)
            {
                this.AddPlistString(boolKey);
            }
            foreach (var boolKey in config.plistKeys.IntKeys)
            {
                this.AddPlistInt(boolKey);
            }
            foreach (var boolKey in config.plistKeys.FloatKeys)
            {
                this.AddPlistFloat(boolKey);
            }
            foreach (var flag in config.flags)
            {
                this.AddBuildProperties(flag);
            }
            foreach (var framework in config.frameworks)
            {
                this.AddFrameworks(framework);
            }
        }

        private void AddFrameworks(iOSFrameworkDescription framework) {
            var exist = false;
            foreach (var existKey in this.frameworks)
            {
                if (existKey.Name == framework.Name)
                {
                    exist = true;
                    break;
                }
            }
            if(exist) return;
            this.frameworks.Add(framework);
        }

        private void AddBuildProperties(BuildProperties prop) {
            var exist = false;
            foreach (var existKey in this.flags)
            {
                if (existKey.Name == prop.Name)
                {
                    exist = true;
                    break;
                }
            }
            if(exist) return;
            this.flags.Add(prop);
        }
        private void AddPlistBool(PlistBoolKey boolKey) {
            var exist = false;
            foreach (var existKey in this.plistKeys.BoolKeys)
            {
                if (existKey.Name == boolKey.Name)
                {
                    exist = true;
                    break;
                }
            }
            if(exist) return;
            this.plistKeys.BoolKeys.Add(boolKey);
        }
        
        private void AddPlistString(PlistStringKey key) {
            var exist = false;
            foreach (var existKey in this.plistKeys.StringKeys)
            {
                if (existKey.Name == key.Name)
                {
                    exist = true;
                    break;
                }
            }
            if(exist) return;
            this.plistKeys.StringKeys.Add(key);
        }
        
        private void AddPlistInt(PlistIntKey key) {
            var exist = false;
            foreach (var existKey in this.plistKeys.IntKeys)
            {
                if (existKey.Name == key.Name)
                {
                    exist = true;
                    break;
                }
            }
            if(exist) return;
            this.plistKeys.IntKeys.Add(key);
        }
        
        private void AddPlistFloat(PlistFloatKey key) {
            var exist = false;
            foreach (var existKey in this.plistKeys.FloatKeys)
            {
                if (existKey.Name == key.Name)
                {
                    exist = true;
                    break;
                }
            }
            if(exist) return;
            this.plistKeys.FloatKeys.Add(key);
        }
    }
    
#region Helper Definition
    static class Tools {
        public static void Process(PostBuildProcessingSettings settings, string path) {
            var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            var target_name = project.GetUnityFrameworkTargetGuid();
            var target = project.TargetGuidByName(target_name);

            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);


            AddFrameworks(project, target, settings.frameworks);
            AddProperty(project, target, settings.flags);
            AddProperties(plist, settings.plistKeys);
            foreach (var copyFilesDirectory in settings.copyFilesDirectories)
            {
                CopyAllFilesFromDirectory(copyFilesDirectory, path, project, target);
            }

            foreach (var newDelegateFile in settings.entitlementsFiles)
            {
                AddEntitlements(newDelegateFile, project, path, target, target_name);
            }

            foreach (var newDelegateFile in settings.newDelegateFiles)
            {
                ReplaceDelegate(newDelegateFile, path);
            }

            project.WriteToFile(projectPath);
            File.WriteAllText(projectPath, project.WriteToString());
            File.WriteAllText(plistPath, plist.WriteToString());
        }

        /// <summary>
        /// Adds properties to XCode project.
        /// For example it's common case to add
        /// a Linker Flag -Objc in OTHER_LDFLAGS to project.
        /// Also Facebook plugin may want you to add -lxml2 flag to OTHER_LDFLAGS.
        /// Both actions can be done using this post process.
        /// </summary>
        public static void AddProperty(PBXProject proj, string targetGUID, List<BuildProperties> properties) {
            if (properties == null) return;
            foreach (var property in properties) {
                proj.AddBuildProperty(targetGUID, property.Name, property.Value);
            }
        }

        /// <summary>
        /// Takes given Entitlements file, copies it to the
        /// Xcode project folder and ads a reference to it
        /// in the project settings
        /// </summary>
        public static void AddEntitlements(DefaultAsset file, PBXProject project, string path, string target, string target_name) {
            if (file == null) return;

            var src = AssetDatabase.GetAssetPath(file);
            var file_name = Path.GetFileName(src);
            var dst = path + "/" + target_name + "/" + file_name;
            FileUtil.CopyFileOrDirectory(src, dst);
            project.AddFile(target_name + "/" + file_name, file_name);
            project.SetBuildProperty(target, "CODE_SIGN_ENTITLEMENTS", target_name + "/" + file_name);
        }

        /// <summary>
        /// Links core frameworks to XCode project,
        /// generated by Unity3D.
        /// 
        /// Examples of valid framework names:
        /// "SystemConfiguration",
        /// "UIKit",
        /// "Foundation",
        /// "CoreTelephony",
        /// "CoreLocation",
        /// "CoreGraphics",
        /// "AdSupport",
        /// "Security",
        /// "GameKit",
        /// "SafariServices"
        /// </summary>
        public static void AddFrameworks(PBXProject project, string target,
            List<iOSFrameworkDescription> frameworksInfo) {
            if (frameworksInfo == null) return;
            foreach (var framework in frameworksInfo)
                project.AddFrameworkToProject(target, framework.Name + ".framework", framework.IsWeak);
        }

        /// <summary>
        /// Writes properties to Info.plist file of XCode project
        /// with given values.
        /// For example:
        /// ITSAppUsesNonExemptEncryption false - claims that your app doesn't use encryption
        /// GADApplicationIdentifier - ca-app-pub-3940256099942544~1458002511 - set Google Ads Plugin app id
        /// GADIsAdManagerApp true - another key, needed to make your app work with Google Ads Plugin
        /// </summary>
        public static void AddProperties(PlistDocument plist, PlistKeys keys) {
            if (keys == null) return;

            if (keys.StringKeys != null) {
                foreach (var key in keys.StringKeys)
                    plist.root.SetString(key.Name, key.Value);
            }

            if (keys.IntKeys != null) {
                foreach (var key in keys.IntKeys)
                    plist.root.SetInteger(key.Name, key.Value);
            }

            if (keys.BoolKeys != null) {
                foreach (var key in keys.BoolKeys)
                    plist.root.SetBoolean(key.Name, key.Value);
            }

            if (keys.FloatKeys != null) {
                foreach (var key in keys.FloatKeys)
                    plist.root.SetReal(key.Name, key.Value);
            }
        }

        /// <summary>
        /// List through all the files in the directory, get path to these files
        /// and add them to 'Copy resources' inside XCode project
        /// </summary>
        public static void CopyAllFilesFromDirectory(DefaultAsset defaultAsset, string buildPath,
            PBXProject project, string target) {
            if (defaultAsset == null) return;
            var files = Directory.GetFiles(AssetDatabase.GetAssetPath(defaultAsset));
            var destinationPath = buildPath + "/";
            foreach (var file in files) {
                var nameAndExtension = file.Split('/').LastOrDefault();
                if (nameAndExtension != null && nameAndExtension.ToLower().Contains(".meta"))
                    continue;
                var assetLocation = AssetDatabase.GetAssetPath(defaultAsset) + "/" + nameAndExtension;
                var assetDestination = destinationPath + nameAndExtension;
                FileUtil.CopyFileOrDirectory(assetLocation, assetDestination);
                var grGUID = project.AddFolderReference(destinationPath + nameAndExtension, nameAndExtension);
                project.AddFileToBuild(target, grGUID);
            }
        }

        private const string DefaultDelegateName = "UnityAppController.mm";

        public static void ReplaceDelegate(DefaultAsset newDelegateFile, string buildPath) {
            if (newDelegateFile == null) return;
            //get paths to new and old delegate
            var newDelegatePath = AssetDatabase.GetAssetPath(newDelegateFile);
            var delegatePath = buildPath + "/Classes/";
            var oldDelegatePath = delegatePath + DefaultDelegateName;

            //remove old delegate, add new one with default unity name
            FileUtil.DeleteFileOrDirectory(oldDelegatePath);
            FileUtil.CopyFileOrDirectory(newDelegatePath, delegatePath + DefaultDelegateName);
        }
    }
#endregion    
#endif
    
#region Data Definitions
    [Serializable]
    public class iOSFrameworkDescription {
        [Delayed] public string Name;
        public bool IsWeak;
    }

    [Serializable]
    public class BuildProperties {
        [Delayed] public string Name;
        [Delayed] public string Value;
    }

    [Serializable]
    public class PlistStringKey {
        [Delayed] public string Name;
        [Delayed] public string Value;
    }

    [Serializable]
    public class PlistBoolKey {
        [Delayed] public string Name;
        public bool Value;
    }

    [Serializable]
    public class PlistIntKey {
        [Delayed] public string Name;
        [Delayed] public int Value;
    }

    [Serializable]
    public class PlistFloatKey {
        [Delayed] public string Name;
        [Delayed] public float Value;
    }

    [Serializable]
    public class PlistKeys {
        public List<PlistStringKey> StringKeys = new List<PlistStringKey>();
        public List<PlistIntKey> IntKeys = new List<PlistIntKey>();
        public List<PlistBoolKey> BoolKeys = new List<PlistBoolKey>();
        public List<PlistFloatKey> FloatKeys = new List<PlistFloatKey>();
    }
#endregion
}
#endif