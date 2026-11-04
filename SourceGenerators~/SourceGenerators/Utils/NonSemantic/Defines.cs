namespace SourceGenerators.Utils.NonSemantic {
    using System.Text;

    public static class Defines {
        public static StringBuilder AppendIfDefine(this StringBuilder sb, string define) {
            sb.Append("#if ").AppendLine(define);
            return sb;
        }
        
        public static StringBuilder AppendEndIfDefine(this StringBuilder sb) {
            sb.AppendLine("#endif");
            return sb;
        }
        
        public static StringBuilder AppendElseDefine(this StringBuilder sb) {
            sb.AppendLine("#else");
            return sb;
        }
    }
}