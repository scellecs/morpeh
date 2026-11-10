namespace SourceGenerators.MorpehHelpers.NonSemantic {
    using System;
    using System.Text;
    using Utils.NonSemantic;

    public static class MorpehSyntax {
        public static ProfileScopeInstance ScopedProfile(StringBuilder sb, string typeName, string suffix, IndentSource indent, bool isUnityProfiler) {
            sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
            sb.AppendIndent(indent).Append(isUnityProfiler ? "UnityEngine.Profiling.Profiler.BeginSample(\"" : "Scellecs.Morpeh.MLogger.BeginSample(\"");
            sb.Append(typeName).Append('_').Append(suffix).AppendLine("\");");
            sb.AppendEndIfDefine();
            
            return new ProfileScopeInstance(sb, indent, isUnityProfiler);
        }
        
        public struct ProfileScopeInstance : IDisposable {
            private StringBuilder sb;
            private IndentSource indent;
            private bool isUnityProfiler;
            
            public ProfileScopeInstance(StringBuilder sb, IndentSource indent, bool isUnityProfiler) {
                this.sb     = sb;
                this.indent = indent;
                this.isUnityProfiler = isUnityProfiler;
            }
            
            public void Dispose() {
                this.sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                this.sb.AppendIndent(this.indent).AppendLine(this.isUnityProfiler ? "UnityEngine.Profiling.Profiler.EndSample();" : "Scellecs.Morpeh.MLogger.EndSample();");
                this.sb.AppendEndIfDefine();
            }
        }
    }
}