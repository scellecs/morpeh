namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Struct)]
    public class EcsComponentAttribute : Attribute {
        public EcsComponentAttribute(int initialCapacity = StashConstants.DEFAULT_COMPONENTS_CAPACITY) {
            
        }
    }
}