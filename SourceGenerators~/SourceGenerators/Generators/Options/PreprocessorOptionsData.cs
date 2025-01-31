﻿namespace SourceGenerators.Generators.Options {
    using Microsoft.CodeAnalysis;

    public record struct PreprocessorOptionsData(
        bool IsUnity,
        bool IsUnityProfiler,
        bool EnableStashSpecialization,
        bool EnableSlowComponentApi
    ) {
        public static PreprocessorOptionsData FromParseOptions(ParseOptions parseOptions) {
            var isUnity                    = false;
            var disableUnityProfiler       = false;
            var disableStashSpecialization = false;
            var enableSlowComponentApi     = false;
            
            foreach (var directive in parseOptions.PreprocessorSymbolNames) {
                switch (directive) {
                    case "UNITY_2019_1_OR_NEWER":
                        isUnity = true;
                        break;
                    case "MORPEH_SOURCEGEN_DISABLE_UNITY_PROFILER":
                        disableUnityProfiler = true;
                        break;
                    case "MORPEH_SOURCEGEN_DISABLE_STASH_SPECIALIZATION":
                        disableStashSpecialization = true;
                        break;
                    case "MORPEH_SOURCEGEN_ENABLE_SLOW_COMPONENT_API":
                        enableSlowComponentApi = true;
                        break;
                }
            }
            
            return new PreprocessorOptionsData(
                IsUnity: isUnity,
                IsUnityProfiler: isUnity && !disableUnityProfiler,
                EnableStashSpecialization: !disableStashSpecialization,
                EnableSlowComponentApi: enableSlowComponentApi
            );
        }
    }
}