namespace SourceGenerators.Generators.SystemsGroup {
    using System.Collections.Generic;
    using MorpehHelpers.NonSemantic;

    public class FieldDefinitionCollection {
        internal readonly List<SystemsGroupFieldDefinition>                     ordered;
        internal readonly Dictionary<string, List<SystemsGroupFieldDefinition>> byLoopType;
            
        public FieldDefinitionCollection() {
            this.ordered    = new List<SystemsGroupFieldDefinition>();
            this.byLoopType = new Dictionary<string, List<SystemsGroupFieldDefinition>>();
        }

        public void Clear() {
            this.ordered.Clear();
                
            foreach (var list in this.byLoopType.Values) {
                list.Clear();
            }
        }
        
        public void Add(SystemsGroupFieldDefinition systemsGroupFieldDefinition) {
            this.ordered.Add(systemsGroupFieldDefinition);
            
            if (!systemsGroupFieldDefinition.loopType.HasValue) {
                return;
            }
            
            if (!this.byLoopType.ContainsKey(systemsGroupFieldDefinition.loopType.Value.MethodName)) {
                this.byLoopType.Add(systemsGroupFieldDefinition.loopType.Value.MethodName, new List<SystemsGroupFieldDefinition>());
            }
            
            this.byLoopType[systemsGroupFieldDefinition.loopType.Value.MethodName].Add(systemsGroupFieldDefinition);
        }
    }
}