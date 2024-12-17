namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class FieldDefinition {
        public FieldDeclarationSyntax? fieldDeclaration;
        public IFieldSymbol?           fieldSymbol;
        public LoopType?               loopType;
        public bool                    isSystem;
        public bool                    isInitializer;
        public bool                    isDisposable;
        public bool                    isInjectable;
        
        public void Reset() {
            this.fieldDeclaration = null;
            this.fieldSymbol      = null;
            this.loopType         = null;
            this.isSystem         = false;
            this.isInitializer    = false;
            this.isDisposable     = false;
            this.isInjectable     = false;
        }
    }
}