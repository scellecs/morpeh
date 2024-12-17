namespace SourceGenerators.Fiddle {
    [AttributeUsage(AttributeTargets.Field)]
    public class LoopAttribute : System.Attribute {
        public LoopAttribute(LoopType loopType) {

        }
    }
}