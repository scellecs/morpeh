namespace Scellecs.Morpeh {
    using System;
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IncludeStashAttribute : Attribute {
        public IncludeStashAttribute(Type type, string customFieldName = "") {
            
        }
    }
}