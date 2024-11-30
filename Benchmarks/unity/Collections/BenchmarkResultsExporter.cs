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
    public class BenchmarkResultsExporter : ScriptableObject
    {
        public TextAsset benchmarkTemplate;
        private const string ResultsFileName = "PerformanceTestResults.json";

        private (string BCL, string Morpeh) GetBenchmarkNames(string className)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == className);
            if (type == null) return (null, null);
            var attr = type.GetCustomAttribute<BenchmarkComparisonAttribute>();
            return attr != null ? (attr.BCL, attr.Morpeh) : (null, null);
        }

        private string GenerateMarkdown(Run runData)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Performance Comparison: Collection Benchmarks");
            sb.AppendLine($"\nBenchmark run on {runData.Hardware.ProcessorType} with {runData.Hardware.ProcessorCount} logical cores.");
            sb.AppendLine($"\nUnity Editor version: {runData.Editor.Version}");
            sb.AppendLine($"\nScripting Backend: {runData.Player.ScriptingBackend}");

            var testsByClass = runData.Results
                .GroupBy(r => r.ClassName)
                .OrderBy(g => g.Key);

            foreach (var classGroup in testsByClass)
            {
                var testName = classGroup.Key.Split('.')[^1].Replace("Benchmark", "");
                var names = GetBenchmarkNames(classGroup.Key);

                sb.AppendLine($"\n### *{testName}*\n");
                sb.AppendLine($"| Functionality | {names.Morpeh} (Morpeh) | {names.BCL} (BCL) |");
                sb.AppendLine("|---|--:|--:|");

                var methodGroups = classGroup
                    .GroupBy(r => (r.MethodName, Param: int.Parse(r.Name.Split('(')[1].Split(',')[0])))
                    .OrderBy(g => g.Key.MethodName)
                    .ThenBy(g => g.Key.Param);

                foreach (var group in methodGroups)
                {
                    var bclResult = group.First(r => r.Name.EndsWith("BCL)"));
                    var morpehResult = group.First(r => r.Name.EndsWith("Morpeh)"));

                    var bclMedian = bclResult.SampleGroups[0].Median;
                    var morpehMedian = morpehResult.SampleGroups[0].Median;
                    var ratio = bclMedian / morpehMedian;
                    var morpehFaster = ratio > 1;

                    sb.AppendLine(
                        $"| `{group.Key.MethodName}({group.Key.Param})` | " +
                        $"{morpehMedian:F3}ms <span style=\"color:{(morpehFaster ? "green" : "red")}\">({ratio:F1}x)</span>&nbsp;{(morpehFaster ? "🟢" : "🟠")} | " +
                        $"*{bclMedian:F3}ms <span style=\"color:{(morpehFaster ? "red" : "green")}\">({(morpehFaster ? "1.0x" : $"{1 / ratio:F1}x")})</span>*&nbsp;{(morpehFaster ? "🟠" : "🟢")} |"
                    );
                }
                sb.AppendLine("\n---");
            }

            return sb.ToString();
        }

        [ContextMenu("Export Results")]
        public void ExportPerformanceTestsResult()
        {
            if (benchmarkTemplate == null)
            {
                Debug.LogError("Benchmark template is not assigned");
                return;
            }

            var resultsPath = Path.Combine(Application.persistentDataPath, ResultsFileName);
            if (!File.Exists(resultsPath))
            {
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
    }
}
#endif