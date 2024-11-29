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
    public class BenchmarkResultsExporter : ScriptableObject
    {
        public TextAsset benchmarkTemplate;
        private const string ResultsFileName = "PerformanceTestResults.json";

        [ContextMenu("111")]
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

        private string GenerateMarkdown(Run runData)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Performance Comparison: Collection Benchmarks");
            sb.AppendLine($"\nBenchmark run on {runData.Hardware.ProcessorType} with {runData.Hardware.ProcessorCount} logical cores.");
            sb.AppendLine($"Unity Editor version: {runData.Editor.Version}");

            var methods = runData.Results
                .GroupBy(r => new { r.MethodName })
                .OrderBy(g => g.Key.MethodName);

            var testsByMethod = methods.ToDictionary(
                group => group.Key.MethodName,
                group => group.GroupBy(r => int.Parse(r.Name.Split('(')[1].Split(',')[0]))
                             .ToDictionary(
                                 g => g.Key,
                                 g => (BCL: g.First(r => r.Name.Contains(",BCL)")),
                                      Morpeh: g.First(r => r.Name.Contains(",Morpeh)")))));

            var testName = runData.Results[0].ClassName.Split('.')[^1].Replace("Benchmark", "");

            sb.AppendLine($"\n### *{testName}*\n");
            sb.AppendLine("| Functionality | FastList<int> (Morpeh) | *List<int> (BCL)* |");
            sb.AppendLine("|---|--:|--:|");

            foreach (var method in methods)
            {
                var tests = testsByMethod[method.Key.MethodName];
                foreach (var test in tests.OrderBy(t => t.Key))
                {
                    var bclMedian = test.Value.BCL.SampleGroups[0].Median;
                    var morpehMedian = test.Value.Morpeh.SampleGroups[0].Median;
                    var ratio = bclMedian / morpehMedian;
                    var speedupColor = ratio > 1 ? "green" : (ratio < 1 ? "red" : "grey");
                    var speedupMark = ratio > 1 ? "🟢" : (ratio < 1 ? "🟠" : "");

                    sb.AppendLine(
                        $"| `{method.Key}({test.Key})` | {morpehMedian:F3}ms <span style=\"color:{speedupColor}\">({ratio:F1}x)</span>&nbsp;{speedupMark} | *{bclMedian:F3}ms <span style=\"color:grey\">(1.0x)</span>*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; |"
                    );
                }
            }

            sb.AppendLine("\n---");
            return sb.ToString();
        }
    }
}
#endif