namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EcsSystemAttribute : Attribute {
        public EcsSystemAttribute(bool skipCommit = false, bool alwaysEnabled = false) {
            
        }
    }
}