namespace SourceGenerators.MorpehHelpers.NonSemantic {
    using System.Runtime.CompilerServices;
    using System.Text;
    using SourceGenerators.Utils.NonSemantic;

    public static class MorpehAttributes {
        public const string COMPONENT_NAME      = "ComponentAttribute";
        public const string COMPONENT_FULL_NAME = "Scellecs.Morpeh.ComponentAttribute";

        public const string STASH_INITIAL_CAPACITY_NAME      = "StashInitialCapacity";
        public const string STASH_INITIAL_CAPACITY_FULL_NAME = "Scellecs.Morpeh.StashInitialCapacityAttribute";

        public const string SYSTEMS_GROUP_NAME      = "SystemsGroupAttribute";
        public const string SYSTEMS_GROUP_FULL_NAME = "Scellecs.Morpeh.SystemsGroupAttribute";

        public const string INLINE_UPDATE_METHODS_NAME      = "SystemsGroupInlineUpdateMethodsAttribute";
        public const string INLINE_UPDATE_METHODS_FULL_NAME = "Scellecs.Morpeh.SystemsGroupInlineUpdateMethodsAttribute";

        public const string SYSTEMS_GROUP_RUNNER_NAME      = "SystemsGroupRunnerAttribute";
        public const string SYSTEMS_GROUP_RUNNER_FULL_NAME = "Scellecs.Morpeh.SystemsGroupRunnerAttribute";

        public const string SYSTEM_NAME      = "SystemAttribute";
        public const string SYSTEM_FULL_NAME = "Scellecs.Morpeh.SystemAttribute";

        public const string ALWAYS_ENABLED_NAME      = "AlwaysEnabledAttribute";
        public const string ALWAYS_ENABLED_FULL_NAME = "Scellecs.Morpeh.AlwaysEnabledAttribute";

        public const string SKIP_COMMIT_NAME      = "SkipCommitAttribute";
        public const string SKIP_COMMIT_FULL_NAME = "Scellecs.Morpeh.SkipCommitAttribute";

        public const string INITIALIZER_NAME      = "InitializerAttribute";
        public const string INITIALIZER_FULL_NAME = "Scellecs.Morpeh.InitializerAttribute";

        public const string INJECTABLE_NAME      = "InjectableAttribute";
        public const string INJECTABLE_FULL_NAME = "Scellecs.Morpeh.InjectableAttribute";

        public const string REGISTER_NAME      = "RegisterAttribute";
        public const string REGISTER_FULL_NAME = "Scellecs.Morpeh.RegisterAttribute";
        
        public static StringBuilder AppendIl2CppAttributes(this StringBuilder sb, IndentSource indent) {
            sb.Append("#if !").Append(MorpehDefines.MORPEH_ENABLE_IL2CPP_CHECKS).Append(" && ").AppendLine(MorpehDefines.ENABLE_IL2CPP);
            sb.AppendIndent(indent).AppendLine("[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]");
            sb.AppendLine("#endif");
            
            return sb;
        }
        
        public static StringBuilder AppendConditionalAttribute(this StringBuilder sb, string conditional, IndentSource indent) {
            sb.AppendIndent(indent).AppendLine($"[System.Diagnostics.Conditional(\"{conditional}\")]");
            
            return sb;
        }
        
        public static StringBuilder AppendInlining(this StringBuilder sb, MethodImplOptions options, IndentSource indent) {
            sb.Append("#if !").Append(MorpehDefines.MORPEH_DISABLE_INLINING).AppendLine();
            sb.AppendIndent(indent).AppendLine($"[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.{options})]");
            sb.AppendLine("#endif");
            
            return sb;
        }
    }
}