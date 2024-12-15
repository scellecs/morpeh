namespace SourceGenerators.Helpers {
    public static class StringHelpers {
        public static string ToCamelCase(this string value) => char.ToLower(value[0]) + value.Substring(1);
    }
}