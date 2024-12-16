namespace SourceGenerators.Generators.SystemsGroup {
    using System.Collections.Generic;

    public class FieldDefinitionCollection {
        internal readonly List<FieldDefinition>                     ordered;
        internal readonly Dictionary<string, List<FieldDefinition>> byLoopType;
            
        public FieldDefinitionCollection() {
            this.ordered    = new List<FieldDefinition>();
            this.byLoopType = new Dictionary<string, List<FieldDefinition>>();
            
            for (int i = 0, length = LoopTypeHelpers.loopMethodNames.Length; i < length; i++) {
                this.byLoopType[LoopTypeHelpers.loopMethodNames[i]] = new List<FieldDefinition>();
            }
        }

        public void Clear() {
            this.ordered.Clear();
                
            for (int i = 0, length = LoopTypeHelpers.loopMethodNames.Length; i < length; i++) {
                this.byLoopType[LoopTypeHelpers.loopMethodNames[i]].Clear();
            }
        }
            
        public void Add(FieldDefinition fieldDefinition) {
            this.ordered.Add(fieldDefinition);
                
            if (fieldDefinition.loopType is not null) {
                this.byLoopType[LoopTypeHelpers.loopMethodNames[(int)fieldDefinition.loopType]].Add(fieldDefinition);
            }
        }
    }
}