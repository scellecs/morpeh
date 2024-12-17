namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;

    public static class LoopTypeHelpers {
        private const string LOOP_ATTRIBUTE = "LoopAttribute";
        
        public static readonly string[] loopMethodNames = {
            "OnEarlyNetworkUpdate",
            "OnFixedUpdate",
            "OnUpdateEverySec",
            "OnNetworkUpdate",
            "OnUpdate",
            "OnLateUpdate",
            "OnCleanupUpdate",
            "OnLateNetworkUpdate",
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