namespace SourceGenerators.Utils.NonSemantic {
    public static class StringExtensions {
        public static string ToCamelCase(this string value) => char.ToLower(value[0]) + value.Substring(1);
    }
}