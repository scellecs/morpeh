namespace Scellecs.Morpeh {
    public interface IAspect {
        Entity Entity { get; set; }
        
        void OnGetAspectFactory(World world);
    }
    
    public partial struct AspectFactory<T> where T : struct, IAspect {
        internal T value;
    }
}
