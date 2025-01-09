namespace SourceGenerators.MorpehHelpers.NonSemantic {
    using System;
    using System.Text;
    using Utils.NonSemantic;

    public static class MorpehSyntax {
        public static ProfileScopeInstance ScopedProfile(StringBuilder sb, string typeName, IndentSource indent) {
            sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
#if MORPEH_SOURCEGEN_UNITY_PROFILER
            sb.AppendIndent(indent).Append("UnityEngine.Profiler.BeginSample(\"");
#else
            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.BeginSample(\"");
#endif
            sb.Append(typeName).AppendLine("\");");
            
            sb.AppendEndIfDefine();
            
            return new ProfileScopeInstance(sb, indent);
        }
        
        public static ProfileScopeInstance ScopedProfile(StringBuilder sb, string typeName, string suffix, IndentSource indent) {
            sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
#if MORPEH_SOURCEGEN_UNITY_PROFILER
            sb.AppendIndent(indent).Append("UnityEngine.Profiler.BeginSample(\"");
#else
            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.BeginSample(\"");
#endif
            sb.Append(typeName).Append('_').Append(suffix).AppendLine("\");");
            
            sb.AppendEndIfDefine();
            
            return new ProfileScopeInstance(sb, indent);
        }
        
        public struct ProfileScopeInstance : IDisposable {
            private StringBuilder sb;
            private IndentSource indent;
            
            public ProfileScopeInstance(StringBuilder sb, IndentSource indent) {
                this.sb     = sb;
                this.indent = indent;
            }
            
            public void Dispose() {
                this.sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
#if MORPEH_SOURCEGEN_UNITY_PROFILER
                this.sb.AppendIndent(indent).AppendLine("UnityEngine.Profiler.EndSample();");
#else
                this.sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.EndSample();");
#endif
                this.sb.AppendEndIfDefine();
            }
        }
    }
}