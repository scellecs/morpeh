namespace SourceGenerators.Fiddle {
    [AttributeUsage(AttributeTargets.Field)]
    public class EcsLoopAttribute : System.Attribute {
        public EcsLoopAttribute(LoopType loopType) {

        }
    }
}