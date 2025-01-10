namespace SourceGenerators.Tests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public static class SourceGeneratorTestUtilities {
    public static void AssertRunCache<T>(CSharpCompilation compilation, string[] trackingNames) where T : IIncrementalGenerator, new() {
        var             generator = new T().AsSourceGenerator();
        var             opts      = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true);
        GeneratorDriver driver    = CSharpGeneratorDriver.Create([generator], driverOptions: opts);

        var clone = compilation.Clone();

        driver = driver.RunGenerators(compilation);

        var runResult1 = driver.GetRunResult();
        var runResult2 = driver.RunGenerators(clone).GetRunResult();

        AssertRunsEqual(runResult1, runResult2, trackingNames);

        var outputs = runResult2.Results[0].TrackedOutputSteps
            .SelectMany(x => x.Value)
            .SelectMany(x => x.Outputs)
            .ToArray();

        Assert.NotEmpty(outputs);
        Assert.All(outputs, x => Assert.Equal(IncrementalStepRunReason.Cached, x.Reason));
    }

    private static void AssertRunsEqual(GeneratorDriverRunResult runResult1, GeneratorDriverRunResult runResult2, string[] trackingNames) {
        var trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        Assert.NotEmpty(trackedSteps1);
        Assert.Equal(trackedSteps1.Count, trackedSteps2.Count);
        Assert.Equal(trackedSteps1.Keys, trackedSteps2.Keys);

        foreach (var (trackingName, runSteps1) in trackedSteps1) {
            var runSteps2 = trackedSteps2[trackingName];
            AssertEqual(runSteps1, runSteps2, trackingName);
        }

        return;

        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(GeneratorDriverRunResult runResult, string[] trackingNames) {
            return runResult.Results[0].TrackedSteps
                .Where(step => trackingNames.Contains(step.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }

    private static void AssertEqual(ImmutableArray<IncrementalGeneratorRunStep> runSteps1, ImmutableArray<IncrementalGeneratorRunStep> runSteps2, string stepName) {
        Assert.Equal(runSteps1.Length, runSteps2.Length);

        for (var i = 0; i < runSteps1.Length; i++) {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            var outputs1 = runStep1.Outputs.Select(x => x.Value).ToArray();
            var outputs2 = runStep2.Outputs.Select(x => x.Value).ToArray();

            Assert.Equal(outputs1.Length, outputs2.Length);
            for (var j = 0; j < outputs1.Length; j++) {
                Assert.Equal(outputs1[j], outputs2[j]);
            }

            Assert.DoesNotContain(runStep2.Outputs, x => x.Reason != IncrementalStepRunReason.Cached && x.Reason != IncrementalStepRunReason.Unchanged);

            AssertObjectGraph(runStep1, stepName);
        }
    }

    private static void AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName) {
        var visited = new HashSet<object>();

        foreach (var (obj, _) in runStep.Outputs) {
            Visit(obj.GetType(), obj);
        }

        return;

        void Visit(Type type, object? node) {
            Assert.False(typeof(Compilation).IsAssignableFrom(type), $"{stepName} must not contain Compilation");
            Assert.False(typeof(ISymbol).IsAssignableFrom(type), $"{stepName} must not contain ISymbol");
            Assert.False(typeof(SyntaxNode).IsAssignableFrom(type), $"{stepName} must not contain SyntaxNode");
            Assert.False(typeof(SemanticModel).IsAssignableFrom(type), $"{stepName} must not contain SemanticModel");
            
            if (node is null || !visited.Add(node)) {
                return;
            }
            
            if (type.IsPrimitive || type.IsEnum || type == typeof(string)) {
                return;
            }

            if (node is IEnumerable collection and not string) {
                foreach (var element in collection) {
                    Visit(element.GetType(), element);
                }

                return;
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                Visit(field.FieldType, field.GetValue(node));
            }
        }
    }
}