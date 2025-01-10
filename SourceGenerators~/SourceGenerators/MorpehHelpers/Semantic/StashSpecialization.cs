namespace SourceGenerators.MorpehHelpers.Semantic {
    public readonly struct StashSpecialization {
        public readonly string type;
        public readonly string getStashMethod;
        public readonly string constraintInterface;
            
        public StashSpecialization(string type, string getStashMethod, string constraintInterface) {
            this.type                = type;
            this.getStashMethod      = getStashMethod;
            this.constraintInterface = constraintInterface;
        }
    }
}