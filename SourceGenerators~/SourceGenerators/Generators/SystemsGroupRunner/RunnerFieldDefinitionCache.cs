namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System;
    using System.Collections.Generic;

    public static class RunnerFieldDefinitionCache {
        [ThreadStatic]
        private static List<RunnerFieldDefinition>? definitions;
        
        public static List<RunnerFieldDefinition> GetList() {
            var list = definitions ??= new List<RunnerFieldDefinition>();
            list.Clear();
            
            return list;
        }
    }
}