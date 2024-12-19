using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace SourceGenerators.Tests;

using Generators.ComponentsMetadata;

[Collection("Sequential")]
public class ComponentSourceGeneratorTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    private const string FILE_NAME = "TestComponent.component_extensions.g.cs";
    
    [Fact]
    public void DataComponent_FileScopedNamespaceWorks() { 
        const string source = """
                              namespace Test.Namespace;

                              using Scellecs.Morpeh;

                              public struct TestComponent : IComponent {
                                  public int value;
                              }
                              """;
        
        const string target = """
                              namespace Test.Namespace {
                              using Scellecs.Morpeh;
                              public static class TestComponent__Generated {
                                  public static Stash<TestComponent> GetStash(World world) => world.GetStash<TestComponent>();
                              }
                              }

                              """;
        
        var result     = Generate(source);
        var syntaxTree = GetGeneratedTree(result, FILE_NAME);

        Assert.Equal(target, syntaxTree.GetText().ToString(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
    
    [Fact]
    public void DataComponent_NamespaceWorks() { 
        const string source = """
                              using Scellecs.Morpeh;
                              
                              namespace Test.Namespace {
                                  public struct TestComponent : IComponent {
                                      public int value;
                                  }
                              }
                              """;
        
        const string target = """
                              namespace Test.Namespace {
                              using Scellecs.Morpeh;
                              public static class TestComponent__Generated {
                                  public static Stash<TestComponent> GetStash(World world) => world.GetStash<TestComponent>();
                              }
                              }
                              
                              """;
        
        var result     = Generate(source);
        var syntaxTree = GetGeneratedTree(result, FILE_NAME);

        Assert.Equal(target, syntaxTree.GetText().ToString(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
    
    [Fact]
    public void DataComponent_GlobalNamespaceWorks() { 
        const string source = """
                              using Scellecs.Morpeh;

                              public struct TestComponent : IComponent {
                                  public int value;
                              }
                              """;
        
        const string target = """
                              
                              using Scellecs.Morpeh;
                              public static class TestComponent__Generated {
                                  public static Stash<TestComponent> GetStash(World world) => world.GetStash<TestComponent>();
                              }
                              
                              
                              """;
        
        var result     = Generate(source);
        var syntaxTree = GetGeneratedTree(result, FILE_NAME);

        Assert.Equal(target, syntaxTree.GetText().ToString(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
    
    [Fact]
    public void DataComponent_GenericWorks() { 
        const string source = """
                              namespace Test.Namespace;
                              
                              using Scellecs.Morpeh;
                              
                              public struct TestComponent<T> : IComponent where T : struct {
                                  public T value;
                              }
                              """;
        
        const string target = """
                              namespace Test.Namespace {
                              using Scellecs.Morpeh;
                              public static class TestComponent__Generated<T>  where T : struct  {
                                  public static Stash<TestComponent<T>> GetStash(World world) => world.GetStash<TestComponent<T>>();
                              }
                              }
                              
                              """;
        
        var result     = Generate(source);
        var syntaxTree = GetGeneratedTree(result, FILE_NAME);

        Assert.Equal(target, syntaxTree.GetText().ToString(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
    
    [Fact]
    public void WrongInterface_DoesNotGenerate() { 
        const string source = """
                              namespace Test.Namespace;
                              
                              using System.ComponentModel; // Instead of Scellecs.Morpeh
                              
                              public class WrongInterface : IComponent {
                                  private ISite? site;
                              
                                  public ISite? Site {
                                      get => this.site;
                                      set => this.site = value;
                                  }
                                  public event EventHandler? Disposed;
                                  
                                  public void Dispose() {
                                  }
                              }
                              """;
        
        var result = Generate(source);
        Assert.Empty(result.GeneratedTrees);
    }
    
    private static GeneratorDriverRunResult Generate(string source) {
        var generator = new ComponentSourceGenerator();
        var driver    = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(ComponentSourceGeneratorTests),
            [
                CSharpSyntaxTree.ParseText(source)
            ],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IComponent).Assembly.Location),
            ]);

        return driver.RunGenerators(compilation).GetRunResult();
    }
    
    private static SyntaxTree GetGeneratedTree(GeneratorDriverRunResult result, string fileName) {
        return result.GeneratedTrees.Single(t => t.FilePath.EndsWith(fileName));
    }
}