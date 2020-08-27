//https://github.com/FredericRP/DependencyUpdater
//
// Author FredericRP Jun 10, 2020
//
// Dependency updater is a simple script that add dependencies for custom packages.
// This is a feature that won't be added into Unity for security reasons.
// To be able to add custom dependencies, you have to add a "customDependencies" array on your package.json file like the sample below.
#if UNITY_EDITOR

namespace Morpeh.Utils.Editor {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEngine;
    using PackageInfo = UnityEditor.PackageManager.PackageInfo;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor.Compilation;

    [InitializeOnLoad]
    public class DependencyResolver : MonoBehaviour {
        [Serializable]
        internal class CustomPackageInfo {
            public string name = default;
            public string version = default;
            public string[] gitDependencies = default;
        }

        static DependencyResolver() {
            CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEventsOnAfterAssemblyReload;
        }

        private static void AssemblyReloadEventsOnAfterAssemblyReload() {
            AssemblyReloadEvents.afterAssemblyReload -= AssemblyReloadEventsOnAfterAssemblyReload;
            ResolveDependencies();
        }

        private static void CompilationPipelineOnAssemblyCompilationStarted(string obj) {
            CompilationPipeline.assemblyCompilationStarted -= CompilationPipelineOnAssemblyCompilationStarted;
            ResolveDependencies();
        }
        
        private static async void ResolveDependencies() {
            var packages = Client.List(true);
            var packagesToAdd = new List<string>();
            while (packages.Status == StatusCode.InProgress) {
                await Task.Yield();
            }

            if (packages.Status != StatusCode.Success) return;
            var gitPackages = new List<PackageInfo>();
            gitPackages.AddRange(packages.Result.Where(package => package.packageId.Contains("ssh") || package.packageId.Contains("http")));
            foreach (var package in packages.Result) {
                var packageFilePath = package.resolvedPath + "\\package.json";
                if (!File.Exists(packageFilePath)) continue;
                var customInfo = JsonUtility.FromJson<CustomPackageInfo>(File.ReadAllText(packageFilePath));
                if(customInfo.gitDependencies == null) continue;

                foreach (var dependency in customInfo.gitDependencies) {
                    var dependencyInfo = dependency.Split('#');
                    var match = gitPackages.Find(p => p.packageId.Contains(dependencyInfo[0]));
                    if(match == null) {
                        packagesToAdd.Add(dependency);
                    }
                    else {
                        var matchInfo = match.packageId.Split('#');
                        if (matchInfo[1].Equals("master")) continue;
                        else if (dependencyInfo[1].Equals("master")) {
                            packagesToAdd.Add(dependency);
                        }
                        else if (string.Compare(dependencyInfo[1], matchInfo[1],
                            StringComparison.Ordinal) > 0) {
                            packagesToAdd.Add(dependency);
                        }
                    }
                }
            }
            
            if (packagesToAdd.Count > 0) {
                var step = 100 / packagesToAdd.Count;
                var progress = 0;
                EditorUtility.DisplayProgressBar("Adding dependencies",
                    "Retrieving " + packagesToAdd.Count + " package(s)", 0 / 100f);
                foreach (var package in packagesToAdd) {
                    var request = Client.Add(package);
                    progress += step;
                    progress = Mathf.Clamp(progress, 0, 100);
                    EditorUtility.DisplayProgressBar("Adding dependencies", "Retrieving [" + package + "]",
                        progress / 100f);
                    while (request.Status == StatusCode.InProgress) {
                        await Task.Yield();
                    }

                    if (request.Status == StatusCode.Success) {
                        ResolveDependencies();
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
#endif