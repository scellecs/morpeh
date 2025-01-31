using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace SourceGenerators.Tests;

using System;
using Diagnostics;
using Generators.Components;
using Generators.Pipelines;
using Microsoft.CodeAnalysis.Testing;
using Utils.NonSemantic;

[Collection("Sequential")]
public class ComponentSourceGeneratorTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    private const string SHARED_CODE = @"namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Struct)]
    public class EcsComponentAttribute : Attribute {
        public EcsComponentAttribute(int initialCapacity = StashConstants.DEFAULT_COMPONENTS_CAPACITY) {
            
        }
    }
}:";
    
    [Fact]
    public void DataComponent_FileScopedNs_NoSpecifiedCapacity() { 
        const string source = """
                              namespace Test.Namespace;
                              
                              using Scellecs.Morpeh;
                              
                              [EcsComponent]
                              public partial struct DataComponent {
                                  public int value;
                              }
                              """;
        
        var result = Generate(source);
        var text = GetGeneratedTree(result, "DataComponent").GetText().ToString();

        if (!text.Contains("public static Stash<DataComponent> GetStash(World world) => world.GetStash<DataComponent>(capacity: 16);")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains(": IDataComponent")) {
            Assert.Fail(text);
        }
    }
    
    [Fact]
    public void DataComponent_FileScopedNs_CustomCapacityPositional() { 
        const string source = """
                              namespace Test.Namespace;
                              
                              using Scellecs.Morpeh;

                              [EcsComponent(32)]
                              public partial struct DataComponent {
                                  public int value;
                              }
                              """;
        
        var result = Generate(source);
        var text   = GetGeneratedTree(result, "DataComponent").GetText().ToString();

        if (!text.Contains("public static Stash<DataComponent> GetStash(World world) => world.GetStash<DataComponent>(capacity: 32);")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains(": IDataComponent")) {
            Assert.Fail(text);
        }
    }
    
    [Fact]
    public void DataComponent_FileScopedNs_CustomCapacityNamed() { 
        const string source = """
                              namespace Test.Namespace;

                              using Scellecs.Morpeh;

                              [EcsComponent(initialCapacity: 32)]
                              public partial struct DataComponent {
                                  public int value;
                              }
                              """;
        
        var result = Generate(source);
        var text   = GetGeneratedTree(result, "DataComponent").GetText().ToString();

        if (!text.Contains("public static Stash<DataComponent> GetStash(World world) => world.GetStash<DataComponent>(capacity: 32);")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains(": IDataComponent")) {
            Assert.Fail(text);
        }
    }
    
    [Fact]
    public void TagComponent() { 
        const string source = """
                              namespace Test.Namespace {
                                using Scellecs.Morpeh;
                              
                                [EcsComponent]
                                public partial struct TagComponent { }
                              }
                              """;
        
        var result = Generate(source);
        var text   = GetGeneratedTree(result, "TagComponent").GetText().ToString();

        if (!text.Contains("public static TagStash GetStash(World world) => world.GetTagStash<TagComponent>(capacity: 16);")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains(": ITagComponent")) {
            Assert.Fail(text);
        }
    }
    
    [Fact]
    public void DisposableComponent() { 
        const string source = """
                              using Scellecs.Morpeh;
                              
                              namespace Test.Namespace {
                                [EcsComponent]
                                public partial struct DisposableComponent {
                                  public int value;
                                  public void Dispose() { }
                                }
                              }
                              """;
        
        var result = Generate(source);
        var text   = GetGeneratedTree(result, "DisposableComponent").GetText().ToString();

        if (!text.Contains("public static DisposableStash<DisposableComponent> GetStash(World world) => world.GetDisposableStash<DisposableComponent>(capacity: 16);")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains(": IDisposableComponent")) {
            Assert.Fail(text);
        }
    }
    
    [Fact]
    public void GenericComponent() { 
        const string source = """
                              using Scellecs.Morpeh;

                              namespace Test.Namespace {
                                [EcsComponent]
                                public partial struct GenericComponent<T> where T : struct {
                                  public T value;
                                }
                              }
                              """;
        
        var result = Generate(source);
        var text   = GetGeneratedTree(result, "GenericComponent").GetText().ToString();

        if (!text.Contains("public static Stash<GenericComponent<T>> GetStash(World world) => world.GetStash<GenericComponent<T>>(capacity: 16);")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains(": IDataComponent")) {
            Assert.Fail(text);
        }
        
        if (!text.Contains("where T : struct")) {
            Assert.Fail(text);
        }
    }
    
    [Fact]
    public void DataComponent_InsideAnotherType() { 
        const string source = """
                              using Scellecs.Morpeh;

                              namespace Test.Namespace {
                                public class SomeClass {
                                  [EcsComponent]
                                  public partial struct GenericComponent<T> where T : struct {
                                    public T value;
                                  }
                                }
                              }
                              """;
        
        var result = Generate(source);
        Assert.Single(result.Diagnostics);
        Assert.Equal(Errors.NESTED_DECLARATION.Id, result.Diagnostics[0].Id);
    }

    [Fact]
    public void SourceGenerator_CachesOutput() {
        const string source = """
                              using Scellecs.Morpeh;

                              namespace Test.Namespace {
                                  [EcsComponent]
                                  public partial struct GenericComponent<T> where T : struct {
                                    public T value;
                                  }
                              }
                              """;
        
        var trackingNames = new[] {TrackingNames.FIRST_PASS, TrackingNames.REMOVE_NULL_PASS };
        SourceGeneratorTestUtilities.AssertRunCache<ComponentsPipeline>(CreateCompilation(source), trackingNames);
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
        var generator = new ComponentsPipeline();
        var driver    = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(CreateCompilation(source)).GetRunResult();
    }
    
    private static SyntaxTree GetGeneratedTree(GeneratorDriverRunResult result, string fileName) {
        return result.GeneratedTrees.Single(t => t.FilePath.Contains(fileName));
    }
}