namespace SourceGenerators.Generators.SystemsGroupRunner {
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public readonly struct RunnerFieldDefinition {
        public readonly FieldDeclarationSyntax? fieldDeclaration;
        public readonly string?                 typeName;
        public readonly string?                 fieldName;
        
        public RunnerFieldDefinition(FieldDeclarationSyntax? fieldDeclaration, string? typeName, string? fieldName) {
            this.fieldDeclaration = fieldDeclaration;
            this.typeName         = typeName;
            this.fieldName        = fieldName;
        }
    }
}