namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class FieldDefinition {
        public FieldDeclarationSyntax fieldDeclaration;
        public IFieldSymbol fieldSymbol;
        public LoopType?    loopType;
        public bool         isSystem;
        public bool         isInitializer;
        public bool         isDisposable;
        public bool         isInjectable;
    }
}