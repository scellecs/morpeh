namespace SourceGenerators.MorpehHelpers.Semantic {
    public record struct StashRequirement(
        string FieldName, 
        string MetadataClassName, 
        StashVariation StashVariation
    );
}