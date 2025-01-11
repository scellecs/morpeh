namespace SourceGenerators.Generators.Systems {
    using System;
    using Microsoft.CodeAnalysis.CSharp;
    using MorpehHelpers.Semantic;
    using Utils.Collections;
    using Utils.NonSemantic;

    public readonly struct InitializerToGenerate : IEquatable<InitializerToGenerate> {
        public readonly string                           typeName;
        public readonly string?                          typeNamespace;
        public readonly string                           genericParams;
        public readonly string                           genericConstraints;
        public readonly EquatableArray<StashRequirement> stashRequirements;
        public readonly TypeDeclType                     typeDeclType;
        public readonly SyntaxKind                       visibility;

        public InitializerToGenerate(
            string typeName,
            string? typeNamespace,
            string genericParams,
            string genericConstraints,
            EquatableArray<StashRequirement> stashRequirements,
            TypeDeclType typeDeclType,
            SyntaxKind visibility) {
            this.typeName           = typeName;
            this.typeNamespace      = typeNamespace;
            this.genericParams      = genericParams;
            this.genericConstraints = genericConstraints;
            this.stashRequirements  = stashRequirements;
            this.typeDeclType       = typeDeclType;
            this.visibility         = visibility;
        }

        public bool Equals(InitializerToGenerate other) => this.typeName == other.typeName &&
                                                           this.typeNamespace == other.typeNamespace &&
                                                           this.genericParams == other.genericParams &&
                                                           this.genericConstraints == other.genericConstraints &&
                                                           this.stashRequirements == other.stashRequirements &&
                                                           this.typeDeclType == other.typeDeclType &&
                                                           this.visibility == other.visibility;

        public override bool Equals(object? obj) => obj is InitializerToGenerate other && this.Equals(other);

        public static bool operator ==(InitializerToGenerate left, InitializerToGenerate right) => left.Equals(right);
        public static bool operator !=(InitializerToGenerate left, InitializerToGenerate right) => !left.Equals(right);

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.typeName.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.typeNamespace != null ? this.typeNamespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.genericParams.GetHashCode();
                hashCode = (hashCode * 397) ^ this.genericConstraints.GetHashCode();
                hashCode = (hashCode * 397) ^ this.stashRequirements.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.typeDeclType;
                hashCode = (hashCode * 397) ^ (int)this.visibility;
                return hashCode;
            }
        }
    }
}