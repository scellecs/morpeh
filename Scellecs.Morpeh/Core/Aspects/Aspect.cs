namespace Scellecs.Morpeh {
    using System;

    [Obsolete("Will be removed in future versions")]
    public interface IAspect {
        Entity Entity { get; set; }
        
        void OnGetAspectFactory(World world);
    }

    [Obsolete("Will be removed in future versions")]
    public interface IBoxedAspectFactory {
        Type AspectType { get; }
        
        IAspect ValueBoxed { get; set; }
    }

    [Obsolete("Will be removed in future versions")]
    public struct AspectFactory<T> : IBoxedAspectFactory where T : struct, IAspect {
        internal T value;
        
        Type IBoxedAspectFactory.AspectType => typeof(T);
        
        IAspect IBoxedAspectFactory.ValueBoxed {
            get => this.value;
            set => this.value = (T) value;
        }
    }
}