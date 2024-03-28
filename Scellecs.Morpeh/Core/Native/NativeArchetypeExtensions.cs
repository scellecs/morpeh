#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class NativeArchetypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Filter.Chunk AsChunk(this Archetype archetype, World world) {
            var len = archetype.length;
            var data = new NativeArray<Entity>(len, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var counter = 0;
            for (var i = 0; i < archetype.length; i++) {
                data[counter++] = archetype.entities[i];
            }
            
            var chunk = new Filter.Chunk {
                entities = (Entity*)data.GetUnsafeReadOnlyPtr(),
                entitiesLength = len,
            };

            world.tempArrays.Add(data);
            
            return chunk;
        }
    }
}
#endif