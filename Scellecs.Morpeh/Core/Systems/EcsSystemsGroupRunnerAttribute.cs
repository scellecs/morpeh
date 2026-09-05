namespace Scellecs.Morpeh {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class EcsSystemsGroupRunnerAttribute : Attribute {
        public EcsSystemsGroupRunnerAttribute(bool generateUpdateMethod = true) {
            
        }
    }
}