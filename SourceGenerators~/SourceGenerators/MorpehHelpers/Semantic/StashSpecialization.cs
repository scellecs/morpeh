namespace SourceGenerators.MorpehHelpers.Semantic {
    public record struct StashSpecialization(
        string Type, 
        string GetStashMethod, 
        string ConstraintInterface
    );
}