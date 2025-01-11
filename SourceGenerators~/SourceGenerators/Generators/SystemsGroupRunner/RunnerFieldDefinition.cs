namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;

    public readonly struct RunnerFieldDefinition {
        public readonly string?                                        typeName;
        public readonly string?                                        fieldName;
        // TODO: EquatableHashSet
        public readonly HashSet<MorpehLoopTypeSemantic.LoopDefinition> loops;
        
        public RunnerFieldDefinition(string? typeName, string? fieldName, HashSet<MorpehLoopTypeSemantic.LoopDefinition> loops) {
            this.typeName         = typeName;
            this.fieldName        = fieldName;
            this.loops            = loops;
        }
    }
}