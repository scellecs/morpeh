namespace SourceGenerators.Generators.SystemsGroup {
    using System.Collections.Generic;

    public class FieldDefinitionCollection {
        internal readonly List<SystemsGroupFieldDefinition>                     ordered;
        internal readonly Dictionary<string, List<SystemsGroupFieldDefinition>> byLoopType;
            
        public FieldDefinitionCollection() {
            this.ordered    = new List<SystemsGroupFieldDefinition>();
            this.byLoopType = new Dictionary<string, List<SystemsGroupFieldDefinition>>();
            
            for (int i = 0, length = LoopTypeHelpers.loopMethodNames.Length; i < length; i++) {
                this.byLoopType[LoopTypeHelpers.loopMethodNames[i]] = new List<SystemsGroupFieldDefinition>();
            }
        }

        public void Clear() {
            this.ordered.Clear();
                
            for (int i = 0, length = LoopTypeHelpers.loopMethodNames.Length; i < length; i++) {
                this.byLoopType[LoopTypeHelpers.loopMethodNames[i]].Clear();
            }
        }
        
        public void Add(SystemsGroupFieldDefinition systemsGroupFieldDefinition) {
            this.ordered.Add(systemsGroupFieldDefinition);
            
            if (systemsGroupFieldDefinition.loopType == null) {
                return;
            }
            
            this.byLoopType[LoopTypeHelpers.loopMethodNames[(int)systemsGroupFieldDefinition.loopType]].Add(systemsGroupFieldDefinition);
        }
    }
}