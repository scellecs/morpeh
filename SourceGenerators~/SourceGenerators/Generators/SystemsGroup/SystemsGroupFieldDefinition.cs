namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class SystemsGroupFieldDefinition {
        public FieldDeclarationSyntax? fieldDeclaration;
        public IFieldSymbol?           fieldSymbol;
        public int?                    loopType;
        public bool                    isSystem;
        public bool                    isInitializer;
        public bool                    isDisposable;
        public bool                    isInjectable;
        public bool                    register;
        public INamedTypeSymbol?       registerAs;
        
        public void Reset() {
            this.fieldDeclaration = null;
            this.fieldSymbol      = null;
            this.loopType         = null;
            this.isSystem         = false;
            this.isInitializer    = false;
            this.isDisposable     = false;
            this.isInjectable     = false;
            this.register         = false;
            this.registerAs       = null;
        }
    }
}