namespace Scellecs.Morpeh {
    using System;
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EcsSystemUpdateMiddlewareAttribute : Attribute {
        public EcsSystemUpdateMiddlewareAttribute(int priority = 0) {
            
        }
    }
}