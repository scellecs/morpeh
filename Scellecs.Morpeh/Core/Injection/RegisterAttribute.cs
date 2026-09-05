namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class RegisterAttribute : Attribute {
        public RegisterAttribute() {
            
        }
        
        public RegisterAttribute(Type type) {
            
        }
    }
}