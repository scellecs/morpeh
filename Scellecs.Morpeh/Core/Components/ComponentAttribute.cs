namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Struct)]
    public class ComponentAttribute : Attribute {
        public ComponentAttribute(int initialCapacity = StashConstants.DEFAULT_COMPONENTS_CAPACITY) {
            
        }
    }
}