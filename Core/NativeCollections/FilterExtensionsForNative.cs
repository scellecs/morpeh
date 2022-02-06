#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using NativeCollections;
    using Unity.Collections;

    public static class FilterExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe NativeFilterWrapper AsNative(this Filter filter) {
            // TODO: Get rid of archetypes NativeArray allocation (?)
            var nativeFilter = new NativeFilterWrapper {
                archetypes = new NativeArray<NativeArchetype>(filter.archetypes.length, Allocator.TempJob),
            };
            
            fixed (int* lengthPtr = &filter.Length) {
                nativeFilter.lengthPtr = lengthPtr;
            }
            
            for (int i = 0, length = filter.archetypes.length; i < length; i++) {
                nativeFilter.archetypes[i] = filter.archetypes.data[i].AsNative();
            }

            return nativeFilter;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter<TNative0> AsNative<TNative0>(this Filter filter)
            where TNative0 : unmanaged, IComponent {
            var nativeComponentsGroup = new NativeFilter<TNative0>();
            nativeComponentsGroup.length      = filter.Length;
            var nativeFilter = filter.AsNative();
            nativeComponentsGroup.components0 = new NativeComponents<TNative0>(nativeFilter, filter.world.GetCache<TNative0>().AsNative());
            return nativeComponentsGroup;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter<TNative0, TNative1> AsNative<TNative0, TNative1>(this Filter filter)
            where TNative0 : unmanaged, IComponent
            where TNative1 : unmanaged, IComponent {
            var nativeComponentsGroup = new NativeFilter<TNative0, TNative1>();
            var nativeFilter          = filter.AsNative();
            nativeComponentsGroup.components0 = new NativeComponents<TNative0>(nativeFilter, filter.world.GetCache<TNative0>().AsNative());
            nativeComponentsGroup.components1 = new NativeComponents<TNative1>(nativeFilter, filter.world.GetCache<TNative1>().AsNative());
            return nativeComponentsGroup;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter<TNative0, TNative1, TNative2> AsNative<TNative0, TNative1, TNative2>(this Filter filter)
            where TNative0 : unmanaged, IComponent
            where TNative1 : unmanaged, IComponent
            where TNative2 : unmanaged, IComponent {
            var nativeComponentsGroup = new NativeFilter<TNative0, TNative1, TNative2>();
            var nativeFilter          = filter.AsNative();
            nativeComponentsGroup.components0 = new NativeComponents<TNative0>(nativeFilter, filter.world.GetCache<TNative0>().AsNative());
            nativeComponentsGroup.components1 = new NativeComponents<TNative1>(nativeFilter, filter.world.GetCache<TNative1>().AsNative());
            nativeComponentsGroup.components2 = new NativeComponents<TNative2>(nativeFilter, filter.world.GetCache<TNative2>().AsNative());
            return nativeComponentsGroup;
        }
    }
}
#endif