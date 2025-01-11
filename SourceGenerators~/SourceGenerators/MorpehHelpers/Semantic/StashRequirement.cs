namespace SourceGenerators.MorpehHelpers.Semantic {
    using System;

    public struct StashRequirement : IEquatable<StashRequirement> {
        public string  fieldName;
        public string  fieldTypeName;
        public string? metadataClassName;
        
        public StashRequirement(string fieldName, string fieldTypeName, string? metadataClassName) {
            this.fieldName         = fieldName;
            this.fieldTypeName     = fieldTypeName;
            this.metadataClassName = metadataClassName;
        }
        
        public bool Equals(StashRequirement other) => this.fieldName == other.fieldName &&
                                                     this.fieldTypeName == other.fieldTypeName &&
                                                     this.metadataClassName == other.metadataClassName;
        
        public override bool Equals(object? obj) => obj is StashRequirement other && this.Equals(other);
        
        public static bool operator ==(StashRequirement left, StashRequirement right) => left.Equals(right);
        public static bool operator !=(StashRequirement left, StashRequirement right) => !left.Equals(right);

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.fieldName.GetHashCode();
                hashCode = (hashCode * 397) ^ this.fieldTypeName.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.metadataClassName != null ? this.metadataClassName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}