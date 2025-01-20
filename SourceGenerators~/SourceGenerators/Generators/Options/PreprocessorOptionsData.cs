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
            
            return new PreprocessorOptionsData(
                IsUnity: symbols.Contains("UNITY_2019_1_OR_NEWER"),
                IsUnityProfiler: symbols.Contains("UNITY_2019_3_OR_NEWER") && !symbols.Contains("MORPEH_SOURCEGEN_NO_UNITY_PROFILER"),
                EnableStashSpecialization: !symbols.Contains("MORPEH_SOURCEGEN_NO_STASH_SPECIALIZATION")
            );
        }
    }
}