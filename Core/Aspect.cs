namespace Scellecs.Morpeh {
    using System;
    
    public interface IAspect {
        Entity Entity { get; set; }
        
        void OnGetAspectFactory(World world);
    }
    
    public interface IBoxedAspectFactory {
        Type AspectType { get; }
        
        IAspect ValueBoxed { get; set; }
    }
    
    public struct AspectFactory<T> : IBoxedAspectFactory where T : struct, IAspect {
        internal T value;
        
        Type IBoxedAspectFactory.AspectType => typeof(T);
        
        IAspect IBoxedAspectFactory.ValueBoxed {
            get => this.value;
            set => this.value = (T) value;
        }
    }
}