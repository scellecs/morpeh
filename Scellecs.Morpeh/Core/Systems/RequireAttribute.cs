namespace Scellecs.Morpeh {
    using System;
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireAttribute : Attribute {
        public RequireAttribute(Type type) {
            
        }
    }
}