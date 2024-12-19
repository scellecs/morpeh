namespace Scellecs.Morpeh {
    using System;

    public class SystemAttribute : Attribute {
        public SystemAttribute(bool skipCommit = false, bool alwaysEnabled = false) {
            
        }
    }
}