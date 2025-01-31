namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EcsSystemsGroupAttribute : Attribute {
        public EcsSystemsGroupAttribute(bool generateUpdateMethod = true, bool inlineUpdateCalls = false) {
            
        }
    }
}