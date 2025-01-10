namespace SourceGenerators.Generators.Components {
    using System;
    using Microsoft.CodeAnalysis.CSharp;
    using MorpehHelpers.Semantic;
    using Utils.Pools;

    public readonly struct ComponentToGenerate : IEquatable<ComponentToGenerate> {
        public readonly string  typeName;
        public readonly string? typeNamespace;
        public readonly string  genericParams;
        public readonly string  genericConstraints;
        public readonly int initialCapacity;
        public readonly StashVariation stashVariation;
        public readonly SyntaxKind visibility;

        public ComponentToGenerate(
            string typeName,
            string? typeNamespace,
            string genericParams,
            string genericConstraints,
            int initialCapacity,
            StashVariation stashVariation,
            SyntaxKind visibility) {
            
            this.typeName            = typeName;
            this.typeNamespace       = typeNamespace;
            this.genericParams       = genericParams;
            this.genericConstraints  = genericConstraints;
            this.initialCapacity     = initialCapacity;
            this.stashVariation      = stashVariation;
            this.visibility          = visibility;
        }
        
        public bool Equals(ComponentToGenerate other) => this.typeName == other.typeName &&
                                                         this.typeNamespace == other.typeNamespace &&
                                                         this.genericParams == other.genericParams &&
                                                         this.genericConstraints == other.genericConstraints &&
                                                         this.initialCapacity == other.initialCapacity &&
                                                         this.stashVariation == other.stashVariation &&
                                                         this.visibility == other.visibility;

        public override bool Equals(object? obj) => obj is ComponentToGenerate other && this.Equals(other);
        
        public static bool operator ==(ComponentToGenerate left, ComponentToGenerate right) => left.Equals(right);
        public static bool operator !=(ComponentToGenerate left, ComponentToGenerate right) => !left.Equals(right);
        
        public override int GetHashCode() {
            unchecked {
                var hashCode = this.typeName.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.typeNamespace != null ? this.typeNamespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.genericParams.GetHashCode();
                hashCode = (hashCode * 397) ^ this.genericConstraints.GetHashCode();
                hashCode = (hashCode * 397) ^ this.initialCapacity;
                hashCode = (hashCode * 397) ^ (int)this.stashVariation;
                hashCode = (hashCode * 397) ^ (int)this.visibility;
                return hashCode;
            }
        }

        public override string ToString() {
            var sb = StringBuilderPool.Get();
            
            sb.Append("typeName = ").Append(this.typeName).Append(", ");
            sb.Append("typeNamespace = ").Append(this.typeNamespace).Append(", ");
            sb.Append("genericParams = ").Append(this.genericParams).Append(", ");
            sb.Append("genericConstraints = ").Append(this.genericConstraints).Append(", ");
            sb.Append("initialCapacity = ").Append(this.initialCapacity).Append(", ");
            sb.Append("stashVariation = ").Append(this.stashVariation);
            sb.Append("visibility = ").Append(this.visibility).Append(", ");
            
            return sb.ToStringAndReturn();
        }
    }
}