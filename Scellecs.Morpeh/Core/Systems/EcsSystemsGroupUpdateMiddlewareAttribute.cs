namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EcsSystemsGroupUpdateMiddlewareAttribute : Attribute {
        public EcsSystemsGroupUpdateMiddlewareAttribute(int priority = 0) {

        }
    }
}
