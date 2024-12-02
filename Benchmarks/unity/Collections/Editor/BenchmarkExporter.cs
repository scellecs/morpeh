#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS && UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.PerformanceTesting.Data;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Scellecs.Morpeh.Benchmarks.Collections.Editor {
    internal static class BenchmarkExporter {
        private const string RESULTS_FILE = "PerformanceTestResults.json";
        private const string README_GUID = "4dc0de86dbf1d814297241c4c82a7f35";

        public static void RunExport() {
            var templatePath = AssetDatabase.GUIDToAssetPath(README_GUID);
            if (!File.Exists(templatePath)) {
                EditorApplication.Exit(1);
                return;
            }

            var resultsPath = Path.Combine(Application.persistentDataPath, RESULTS_FILE);
            if (!File.Exists(resultsPath)) {
                Debug.LogError($"Results file not found at: {resultsPath}");
                EditorApplication.Exit(1);
                return;
            }

            try {
                var jsonData = File.ReadAllText(resultsPath);
                var runData = JsonUtility.FromJson<Run>(jsonData);
                var markdownData = GenerateMarkdown(runData);

                File.WriteAllText(templatePath, markdownData);
                AssetDatabase.Refresh();

                Debug.Log("Export completed successfully");
                EditorApplication.Exit(0);
            }
            catch (Exception ex) {
                Debug.LogError($"Error during export: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }

        private class BenchmarkData {
            public Type EnumType;
            public string[] Names;
        }

        private static BenchmarkData GetBenchmarkData(string className) {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == className);

            if (type == null) {
                return null;
            }

            var attr = type.GetCustomAttribute<BenchmarkComparisonAttribute>();
            if (attr == null) {
                return null;
            }

            return new BenchmarkData {
                EnumType = attr.EnumType,
                Names = attr.Names,
            };
        }

        private static string GenerateMarkdown(Run runData) {
            var sb = new StringBuilder();
            GenerateSetupInstructions(sb);
            sb.AppendLine($"# Performance Comparison: Collection Benchmarks");
            sb.AppendLine($"\n## Environment");
            sb.AppendLine($"\nBenchmark run on {runData.Hardware.ProcessorType} with {runData.Hardware.ProcessorCount} logical cores.");
            sb.AppendLine($"\nUnity Editor version: {runData.Editor.Version}");
            sb.AppendLine($"\nScripting Backend: {runData.Player.ScriptingBackend}");

            sb.AppendLine($"\n## Results");

            var testsByClass = runData.Results
                .GroupBy(r => r.ClassName)
                .OrderBy(g => g.Key);

            foreach (var classGroup in testsByClass) {
                var testName = classGroup.Key.Split('.')[^1].Replace("Benchmark", "");
                var benchmarkData = GetBenchmarkData(classGroup.Key);

                if (benchmarkData == null) {
                    continue;
                }

                sb.AppendLine($"\n### *{testName}*\n");
                sb.Append("| Functionality |");

                for (int i = 0; i < benchmarkData.Names.Length; i++) {
                    var enumValue = Enum.GetName(benchmarkData.EnumType, i);
                    sb.Append($" {benchmarkData.Names[i]} ({enumValue}) |");
                }

                sb.AppendLine();
                sb.Append("|---|");

                for (int i = 0; i < benchmarkData.Names.Length; i++) {
                    sb.Append("--:|");
                }

                sb.AppendLine();

                var methodGroups = classGroup
                    .GroupBy(r => (r.MethodName, Param: int.Parse(r.Name.Split('(')[1].Split(',')[0])))
                    .OrderBy(g => g.Key.MethodName)
                    .ThenBy(g => g.Key.Param);

                foreach (var group in methodGroups) {
                    var results = new List<double>();
                    for (int i = 0; i < benchmarkData.Names.Length; i++) {
                        var enumName = Enum.GetName(benchmarkData.EnumType, i);
                        var result = group.First(r => r.Name.EndsWith($"{enumName})"));
                        results.Add(result.SampleGroups[0].Median);
                    }

                    var worstResult = results.Max();
                    var bestResult = results.Min();
                    sb.Append($"| `{group.Key.MethodName}({group.Key.Param})` |");

                    for (int i = 0; i < results.Count; i++) {
                        var isBest = Math.Abs(results[i] - bestResult) < 0.001d;
                        sb.Append($" {results[i]:F3}ms ");
                        if (isBest) {
                            var speedup = worstResult / bestResult;
                            sb.Append($"<span style=\"color:green\">({speedup:F1}x)</span>");
                        }
                        else {
                            sb.Append("<span style=\"color:red\">(1.0x)</span>");
                        }
                        sb.Append($"&nbsp;{(isBest ? "🟢" : "🟠")} |");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("\n---");
            }

            return sb.ToString();
        }

        private static void GenerateSetupInstructions(StringBuilder sb) {
            sb.AppendLine("## Setup Instructions");
            sb.AppendLine("\nBefore running the benchmarks, complete these steps:");
            sb.AppendLine("\n1. Project Setup:");
            sb.AppendLine("   - Create a clean Unity project");
            sb.AppendLine("   - Clone Morpeh into the Assets folder");
            sb.AppendLine("   - Install package: ``com.unity.test-framework.performance``");
            sb.AppendLine("\n2. Project Settings Configuration:");
            sb.AppendLine("   - Disable VSync");
            sb.AppendLine("   - Remove all Quality Settings except one");
            sb.AppendLine("   - Set Scripting Backend to IL2CPP");
            sb.AppendLine("   - Remove the camera from the scene");
            sb.AppendLine("   - Close Unity Editor (not needed anymore)");
            sb.AppendLine("\n3. Running Benchmarks (Windows Example):");
            sb.AppendLine("   - Open terminal");
            sb.AppendLine("   - Navigate to Unity Editor folder, e.g.:");
            sb.AppendLine("     ```");
            sb.AppendLine("     cd \"C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.49f1\\Editor\"");
            sb.AppendLine("     ```");
            sb.AppendLine("   - Run build and tests command:");
            sb.AppendLine("     ```");
            sb.AppendLine("     ./Unity.exe -runTests -batchMode -projectPath PROJECT_PATH -testPlatform StandaloneWindows64 -buildTarget StandaloneWindows64 -mtRendering -scriptingbackend=il2cpp");
            sb.AppendLine("     ```");
            sb.AppendLine("     Replace PROJECT_PATH with your path (e.g., M:/morpeh-2024)");
            sb.AppendLine("\n4. Export Results:");
            sb.AppendLine("   - After tests complete and Unity application closes, run:");
            sb.AppendLine("     ```");
            sb.AppendLine("     ./Unity.exe -batchmode -projectPath PROJECT_PATH -executeMethod Scellecs.Morpeh.Benchmarks.Collections.Editor.BenchmarkExporter.RunExport -quit");
            sb.AppendLine("     ```");
            sb.AppendLine("     Again, replace PROJECT_PATH with your actual project path");
        }
    }
}
#endif