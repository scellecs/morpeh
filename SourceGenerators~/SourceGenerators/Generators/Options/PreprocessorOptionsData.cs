namespace SourceGenerators.Generators.Options {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    public record struct PreprocessorOptionsData(
        bool IsUnity,
        bool IsUnityProfiler,
        bool EnableStashSpecialization
    ) {
        public static PreprocessorOptionsData FromParseOptions(ParseOptions parseOptions) {
            var symbols = parseOptions.PreprocessorSymbolNames.ToImmutableHashSet();

            var isUnity = symbols.Contains("UNITY_2019_1_OR_NEWER");
            
            return new PreprocessorOptionsData(
                IsUnity: isUnity,
                IsUnityProfiler: isUnity && !symbols.Contains("MORPEH_SOURCEGEN_NO_UNITY_PROFILER"),
                EnableStashSpecialization: !symbols.Contains("MORPEH_SOURCEGEN_NO_STASH_SPECIALIZATION")
            );
        }
    }
}