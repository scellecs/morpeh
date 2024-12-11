//https://github.com/FredericRP/DependencyUpdater
//
// Author FredericRP Jun 10, 2020
//
// Dependency updater is a simple script that add dependencies for custom packages.
// This is a feature that won't be added into Unity for security reasons.
// To be able to add custom dependencies, you have to add a "customDependencies" array on your package.json file like the sample below.

#if UNITY_EDITOR

namespace Scellecs.Morpeh.Utils.Editor {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEditor.PackageManager;
    using UnityEngine;
    using PackageInfo = UnityEditor.PackageManager.PackageInfo;

    [InitializeOnLoad]
    public class DependencyResolver : MonoBehaviour {
        [Serializable]
        internal class CustomPackageInfo {
            public string[] simpleGitDependencies = default;
        }

        static DependencyResolver() {
            //todo
            //UnityEditor.PackageManager.Events.registeredPackages replace for unity 2020+
            //we still support 2019.4 so we use assemblyReload event
 #pragma warning disable CS0618
            CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
 #pragma warning restore CS0618
            AssemblyReloadEvents.afterAssemblyReload       += AssemblyReloadEventsOnAfterAssemblyReload;
        }

        private static void AssemblyReloadEventsOnAfterAssemblyReload() {
            AssemblyReloadEvents.afterAssemblyReload -= AssemblyReloadEventsOnAfterAssemblyReload;
            ResolveDependencies();
        }

        private static void CompilationPipelineOnAssemblyCompilationStarted(string obj) {
 #pragma warning disable CS0618
            CompilationPipeline.assemblyCompilationStarted -= CompilationPipelineOnAssemblyCompilationStarted;
 #pragma warning restore CS0618
            ResolveDependencies();
           
        }

        private static async void ResolveDependencies() {
            var packages      = Client.List(true);
            var packagesToAdd = new List<string>();
            while (packages.Status == StatusCode.InProgress) {
                await Task.Yield();
            }

            if (packages.Status != StatusCode.Success) return;
            var gitPackages = new List<PackageInfo>();
            gitPackages.AddRange(packages.Result.Where(package => package.packageId.Contains("ssh") || package.packageId.Contains("http")));
            foreach (var package in packages.Result) {
                var packageFilePath = package.resolvedPath + Path.DirectorySeparatorChar + "package.json";
                if (!File.Exists(packageFilePath)) continue;
                var customInfo = JsonUtility.FromJson<CustomPackageInfo>(File.ReadAllText(packageFilePath));
                if (customInfo.simpleGitDependencies == null) continue;

                foreach (var dependency in customInfo.simpleGitDependencies) {
                    var dependencyInfo   = dependency.Split('#');
                    var dependencyBranch = dependencyInfo.Length > 1 ? dependencyInfo[1] : string.Empty;

                    var match = gitPackages.Find(p => p.packageId.Contains(dependencyInfo[0]));
                    if (match == null) {
                        packagesToAdd.Add(dependency);
                    }
                    else {
                        var matchInfo   = match.packageId.Split('#');
                        var matchBranch = matchInfo.Length > 1 ? matchInfo[1] : string.Empty;

                        if (matchBranch.Equals("master") || matchBranch.Equals("main")) {
                        }
                        else if (dependencyBranch.Equals("master") || dependencyBranch.Equals("main")) {
                            packagesToAdd.Add(dependency);
                        }
                        else if (string.Compare(dependencyBranch, matchBranch,
                            StringComparison.Ordinal) > 0) {
                            packagesToAdd.Add(dependency);
                        }
                    }
                }
            }

            if (packagesToAdd.Count > 0) {
                var step     = 100 / packagesToAdd.Count;
                var progress = 0;
                EditorUtility.DisplayProgressBar("Adding dependencies",
                    "Retrieving " + packagesToAdd.Count + " package(s)", 0 / 100f);
                foreach (var package in packagesToAdd) {
                    var request = Client.Add(package);
                    progress += step;
                    progress =  Mathf.Clamp(progress, 0, 100);
                    EditorUtility.DisplayProgressBar("Adding dependencies", "Retrieving [" + package + "]",
                        progress / 100f);
                    while (request.Status == StatusCode.InProgress) {
                        await Task.Yield();
                    }

                    if (request.Status == StatusCode.Success) {
                        ResolveDependencies();
                    }
                    else {
                        Debug.LogError(request.Error.message);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
#endif