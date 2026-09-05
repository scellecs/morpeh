namespace Scellecs.Morpeh {
    using System;
#if !MORPEH_SUPPRESS_OBSOLETE
    [Obsolete("Will be removed in future versions")]
#endif
    public interface IAspect {
        Entity Entity { get; set; }
        
        void OnGetAspectFactory(World world);
    }
#if !MORPEH_SUPPRESS_OBSOLETE
    [Obsolete("Will be removed in future versions")]
#endif
    public interface IBoxedAspectFactory {
        Type AspectType { get; }
        
        IAspect ValueBoxed { get; set; }
    }
#if !MORPEH_SUPPRESS_OBSOLETE
    [Obsolete("Will be removed in future versions")]
#endif
    public struct AspectFactory<T> : IBoxedAspectFactory where T : struct, IAspect {
        internal T value;
        
        Type IBoxedAspectFactory.AspectType => typeof(T);
        
        IAspect IBoxedAspectFactory.ValueBoxed {
            get => this.value;
            set => this.value = (T) value;
        }
    }
}