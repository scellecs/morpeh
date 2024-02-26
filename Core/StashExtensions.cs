#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using Collections;
    public static class StashExtensions {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
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
#endif

        public static Stash<T> Clone<T>(this Stash<T> stash) where T : unmanaged, IComponent {
#if MORPEH_DEBUG
            if (stash == null || stash.components == null) {
                throw new Exception($"[MORPEH] You are trying clone null or disposed stash");
            }
#endif
            
            return new Stash<T>(stash);
        }
        
        public static void CopyFrom<T>(this Stash<T> stash, Stash<T> from) where T : unmanaged, IComponent {
#if MORPEH_DEBUG
            if (stash == null || from == null || stash.components == null || from.components == null) {
                throw new Exception($"[MORPEH] You are trying copy from null or disposed stash");
            }
#endif
            
            stash.components.CopyFrom(from.components);
        }
    }
}
