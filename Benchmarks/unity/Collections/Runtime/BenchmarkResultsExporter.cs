#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS && UNITY_EDITOR
using UnityEngine;
using System;
using System.IO;
using Unity.PerformanceTesting.Data;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using System.Reflection;

namespace Scellecs.Morpeh.Benchmarks.Collections {
    [CreateAssetMenu(fileName = "BenchmarkResults", menuName = "Benchmarks/Results Exporter")]
    public sealed class BenchmarkResultsExporter : ScriptableObject {
        public TextAsset benchmarkTemplate;
        private const string RESULTS_FILE = "PerformanceTestResults.json";

        private sealed class BenchmarkData {
            public Type EnumType;
            public string[] Names;
        }

        public void ExportPerformanceTestsResult() {
            if (benchmarkTemplate == null) {
                Debug.LogError("Benchmark template is not assigned");
                return;
            }

            var resultsPath = Path.Combine(Application.persistentDataPath, RESULTS_FILE);

            if (!File.Exists(resultsPath)) {
                Debug.LogError($"Results file not found at: {resultsPath}");
                return;
            }

            var jsonData = File.ReadAllText(resultsPath);
            var runData = JsonUtility.FromJson<Run>(jsonData);
            var markdownData = GenerateMarkdown(runData);

            var assetPath = AssetDatabase.GetAssetPath(benchmarkTemplate);
            File.WriteAllText(assetPath, markdownData);
            AssetDatabase.Refresh();
        }

        private BenchmarkData GetBenchmarkData(string className) {
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

        private string GenerateMarkdown(Run runData) {
            var sb = new StringBuilder();
            sb.AppendLine($"# Performance Comparison: Collection Benchmarks");
            sb.AppendLine($"\nBenchmark run on {runData.Hardware.ProcessorType} with {runData.Hardware.ProcessorCount} logical cores.");
            sb.AppendLine($"\nUnity Editor version: {runData.Editor.Version}");
            sb.AppendLine($"\nScripting Backend: {runData.Player.ScriptingBackend}");

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
    }
}
#endif