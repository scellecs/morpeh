namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;

    public static class LoopTypeHelpers {
        private const string LOOP_ATTRIBUTE = "LoopAttribute";
        
        // Predefined names. Change them if you need to and keep your enum mapping updated.
        public static readonly string[] loopMethodNames = {
            "OnUpdate",        // 0
            "OnFixedUpdate",   // 1
            "OnLateUpdate",    // 2
            "OnCleanupUpdate", // 3
            
            "OnEarlyNetworkUpdate", // 4
            "OnLateNetworkUpdate",  // 5
            "OnNetworkUpdate",      // 6
            
            "OnUpdateEverySec",     // 7
            
            "OnTick", // 8
        };

        public static int? GetLoopMethodNameFromField(IFieldSymbol fieldSymbol) {
            var attributes = fieldSymbol.GetAttributes();

            for (int i = 0, length = attributes.Length; i < length; i++) {
                var attribute = attributes[i];
                
                if (attribute.AttributeClass?.Name != LOOP_ATTRIBUTE) {
                    continue;
                }
                
                if (attribute.ConstructorArguments.Length == 0) {
                    continue;
                }

                var loopType = attribute.ConstructorArguments[0].Value;
                if (loopType == null) {
                    continue;
                }
                
                return (int)loopType;
            }

            return null;
        }
    }
}