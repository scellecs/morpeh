namespace SourceGenerators.Helpers {
    public readonly struct StashSpecialization {
        public readonly string type;
        public readonly string getStashMethod;
            
        public StashSpecialization(string type, string getStashMethod) {
            this.type = type;
            this.getStashMethod = getStashMethod;
        }
    }
}