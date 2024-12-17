namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public readonly struct RunnerFieldDefinition {
        public readonly string?                 typeName;
        public readonly string?                 fieldName;
        public readonly HashSet<string>         loops;
        
        public RunnerFieldDefinition(string? typeName, string? fieldName, HashSet<string> loops) {
            this.typeName         = typeName;
            this.fieldName        = fieldName;
            this.loops            = loops;
        }
    }
}