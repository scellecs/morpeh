namespace SourceGenerators.Helpers {
    using System.Text;

    public static class StringBuilderExtensions {
        public static StringBuilder AppendIndent(this StringBuilder sb, int indent) {
            sb.Append(' ', indent);
            return sb;
        }
        
        public static StringBuilder AppendIndent(this StringBuilder sb, IndentSource indentSource) {
            sb.Append(' ', indentSource.value);
            return sb;
        }
    }
}