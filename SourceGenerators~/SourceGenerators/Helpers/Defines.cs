namespace SourceGenerators.Helpers {
    using System.Text;

    public static class Defines {
        public const string MORPEH_DEBUG = "MORPEH_DEBUG";
        public const string MORPEH_PROFILING = "MORPEH_PROFILING";
        
        public static StringBuilder AppendMorpehDebugDefines(this StringBuilder sb) {
            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine("#define MORPEH_DEBUG");
            sb.AppendLine("#define MORPEH_PROFILING");
            sb.AppendLine("#endif");
            sb.AppendLine("#if !MORPEH_DEBUG");
            sb.AppendLine("#define MORPEH_DEBUG_DISABLED");
            sb.AppendLine("#endif");
            return sb;
        }
        
        public static StringBuilder AppendMorpehUnityDefines(this StringBuilder sb) {
            sb.AppendLine("#if ENABLE_MONO || ENABLE_IL2CPP");
            sb.AppendLine("#define MORPEH_UNITY");
            sb.AppendLine("#endif");
            return sb;
        }
        
        public static StringBuilder AppendIfDefine(this StringBuilder sb, string define) {
            sb.Append("#if ").AppendLine(define);
            return sb;
        }
        
        public static StringBuilder AppendEndIfDefine(this StringBuilder sb) {
            sb.AppendLine("#endif");
            return sb;
        }
        
        public static StringBuilder AppendElseIfDefine(this StringBuilder sb, string define) {
            sb.Append("#elif ").AppendLine(define);
            return sb;
        }
        
        public static StringBuilder AppendElseDefine(this StringBuilder sb) {
            sb.AppendLine("#else");
            return sb;
        }
    }
}