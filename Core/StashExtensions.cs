#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    public static class StashExtensions {
        public static Stash<T> AsDisposable<T>(this Stash<T> stash) where T : struct, IComponent, IDisposable {
#if MORPEH_DEBUG
            if (stash == null || stash.components == null) {
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
    }
}
