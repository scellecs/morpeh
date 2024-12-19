namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SystemsGroupAttribute : Attribute {
        public SystemsGroupAttribute(bool inlineUpdateCalls = false) {
            
        }
    }
}