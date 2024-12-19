namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SystemAttribute : Attribute {
        public SystemAttribute(bool skipCommit = false, bool alwaysEnabled = false) {
            
        }
    }
}