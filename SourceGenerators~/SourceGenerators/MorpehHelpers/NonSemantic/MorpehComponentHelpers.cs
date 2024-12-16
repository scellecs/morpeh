namespace SourceGenerators.MorpehHelpers.NonSemantic {
    public static class MorpehComponentHelpers {
        public static string ComponentNameToMetadataClassName(string componentName) {
            var index = componentName.IndexOf('<');
            return index > 0 ? componentName.Insert(index, "__Metadata") : $"{componentName}__Metadata";
        }
    }
}