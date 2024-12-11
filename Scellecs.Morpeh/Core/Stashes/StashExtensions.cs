#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class StashExtensions {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        public static Stash<T> AsDisposable<T>(this Stash<T> stash) where T : struct, IComponent, IDisposable {
#if MORPEH_DEBUG
            if (stash == null || stash.IsDisposed) {
                throw new Exception($"[MORPEH] You are trying mark AsDisposable null or disposed stash");
            }
#endif
            
            if (stash.componentDispose != null) {
                return stash;
            }
            
            void ComponentDispose(ref T c) => c.Dispose();
            stash.componentDispose = ComponentDispose;

            return stash;
        }
#endif
    }
}
