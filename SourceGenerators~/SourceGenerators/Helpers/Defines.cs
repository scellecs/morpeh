namespace SourceGenerators.Helpers {
    using System.Text;

    public static class Defines {
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
    }
}