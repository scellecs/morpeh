namespace SourceGenerators.Helpers {
    public readonly struct StashSpecialization {
        public readonly string type;
        public readonly string getStashMethod;

        public readonly bool isTag;
        public readonly bool isDisposable;
            
        public StashSpecialization(string type, string getStashMethod, bool isTag, bool isDisposable) {
            this.type = type;
            this.getStashMethod = getStashMethod;
            this.isTag = isTag;
            this.isDisposable = isDisposable;
        }
    }
}