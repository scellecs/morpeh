namespace SourceGenerators.Generators.Options {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    public record struct PreprocessorOptionsData(
        bool IsUnity,
        bool EnableStashSpecialization
    ) {
        public static PreprocessorOptionsData FromParseOptions(ParseOptions parseOptions) {
            var symbols = parseOptions.PreprocessorSymbolNames.ToImmutableHashSet();
            
            return new PreprocessorOptionsData(
                IsUnity: symbols.Contains("UNITY_2019_1_OR_NEWER"), // TODO: Use this information in profiling generation
                EnableStashSpecialization: !symbols.Contains("MORPEH_SOURCEGEN_NO_STASH_SPECIALIZATION")
            );
        }
    }
}