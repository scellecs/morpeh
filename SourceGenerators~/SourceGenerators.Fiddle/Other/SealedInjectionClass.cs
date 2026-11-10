namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[Injectable]
public sealed partial class SealedInjectionClass {
    [Injectable]
    private IDisposable _disposable_base1;
    
    [Injectable]
    private IDisposable _disposable_base2;
    
    [Injectable]
    private BasicGenericClass<string> _generic;
}