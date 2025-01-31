using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace SourceGenerators.Tests;

using Generators.Systems;
using Utils.NonSemantic;

[Collection("Sequential")]
public class InitializerSourceGeneratorTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    private const string SHARED_CODE = @"namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EcsInitializerAttribute : Attribute {
        public EcsInitializerAttribute() {
            
        }
    }
}:";
    
    [Fact]
    public void SourceGenerator_CachesOutput() {
        const string source = """
                              using Scellecs.Morpeh;

                              namespace Test.Namespace {
                                  [EcsInitializer]
                                  public partial class SomeInitializer {
                                      public World World { get; }

                                      public void OnAwake() {
                                          throw new NotImplementedException();
                                      }

                                      public void Dispose() {
                                          throw new NotImplementedException();
                                      }
                                  }
                              }
                              """;
        
        var trackingNames = new[] {TrackingNames.FIRST_PASS, TrackingNames.REMOVE_NULL_PASS };
        SourceGeneratorTestUtilities.AssertRunCache<InitializerSourceGenerator>(CreateCompilation(source), trackingNames);
    }

    private static CSharpCompilation CreateCompilation(string source) =>
        CSharpCompilation.Create(nameof(ComponentSourceGeneratorTests),
            [
                CSharpSyntaxTree.ParseText(source),
                CSharpSyntaxTree.ParseText(SHARED_CODE),
            ],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IComponent).Assembly.Location),
            ]);

    private static GeneratorDriverRunResult Generate(string source) {
        var generator = new InitializerSourceGenerator();
        var driver    = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(CreateCompilation(source)).GetRunResult();
    }
    
    private static SyntaxTree GetGeneratedTree(GeneratorDriverRunResult result, string fileName) {
        return result.GeneratedTrees.Single(t => t.FilePath.Contains(fileName));
    }
}