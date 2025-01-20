namespace SourceGenerators.MorpehHelpers.NonSemantic {
    using System.Runtime.CompilerServices;
    using System.Text;
    using SourceGenerators.Utils.NonSemantic;

    public static class MorpehAttributes {
        public const string COMPONENT_NAME      = "EcsComponentAttribute";
        public const string COMPONENT_FULL_NAME = "Scellecs.Morpeh.EcsComponentAttribute";

        public const string SYSTEMS_GROUP_NAME      = "EcsSystemsGroupAttribute";
        public const string SYSTEMS_GROUP_FULL_NAME = "Scellecs.Morpeh.EcsSystemsGroupAttribute";

        public const string SYSTEMS_GROUP_RUNNER_NAME      = "EcsSystemsGroupRunnerAttribute";
        public const string SYSTEMS_GROUP_RUNNER_FULL_NAME = "Scellecs.Morpeh.EcsSystemsGroupRunnerAttribute";

        public const string SYSTEM_NAME      = "EcsSystemAttribute";
        public const string SYSTEM_FULL_NAME = "Scellecs.Morpeh.EcsSystemAttribute";

        public const string INITIALIZER_NAME      = "EcsInitializerAttribute";
        public const string INITIALIZER_FULL_NAME = "Scellecs.Morpeh.EcsInitializerAttribute";
        
        public const string INJECTABLE_NAME      = "InjectableAttribute";
        public const string INJECTABLE_FULL_NAME = "Scellecs.Morpeh.InjectableAttribute";
        
        public const string GENERIC_INJECTION_RESOLVER_NAME                = "GenericInjectionResolverAttribute";
        public const string GENERIC_INJECTION_RESOLVER_ATTRIBUTE_FULL_NAME = "Scellecs.Morpeh.GenericInjectionResolverAttribute";

        public const string REGISTER_NAME      = "RegisterAttribute";
        public const string REGISTER_FULL_NAME = "Scellecs.Morpeh.RegisterAttribute";
        
        public const string INCLUDE_STASH_NAME      = "IncludeStashAttribute";
        public const string INCLUDE_STASH_FULL_NAME = "Scellecs.Morpeh.IncludeStashAttribute";
        
        public const string MONO_PROVIDER_NAME      = "MonoProviderAttribute";
        public const string MONO_PROVIDER_FULL_NAME = "Scellecs.Morpeh.Providers.MonoProviderAttribute";
        
        public static StringBuilder AppendIl2CppAttributes(this StringBuilder sb, IndentSource indent) {
            sb.Append("#if !").Append(MorpehDefines.MORPEH_ENABLE_IL2CPP_CHECKS).Append(" && ").AppendLine(MorpehDefines.ENABLE_IL2CPP);
            sb.AppendIndent(indent).AppendLine("[global::Unity.IL2CPP.CompilerServices.Il2CppSetOption(global::Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[global::Unity.IL2CPP.CompilerServices.Il2CppSetOption(global::Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[global::Unity.IL2CPP.CompilerServices.Il2CppSetOption(global::Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]");
            sb.AppendLine("#endif");
            
            return sb;
        }
        
        public static StringBuilder AppendConditionalAttribute(this StringBuilder sb, string conditional, IndentSource indent) {
            sb.AppendIndent(indent).AppendLine($"[global::System.Diagnostics.Conditional(\"{conditional}\")]");
            
            return sb;
        }
        
        public static StringBuilder AppendInlining(this StringBuilder sb, MethodImplOptions options, IndentSource indent) {
            sb.Append("#if !").Append(MorpehDefines.MORPEH_DISABLE_INLINING).AppendLine();
            sb.AppendIndent(indent).AppendLine($"[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.{options})]");
            sb.AppendLine("#endif");
            
            return sb;
        }
    }
}