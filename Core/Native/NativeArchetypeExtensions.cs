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
        internal static Filter.Chunk AsChunk(this Archetype archetype) {
            var len = archetype.entities.count;
            var data = new NativeArray<int>(len, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var counter = 0;
            foreach (var entityId in archetype.entities) {
                data[counter++] = entityId;
            }
            
            var chunk = new Filter.Chunk {
                entities = (int*)data.GetUnsafeReadOnlyPtr(),
                entitiesLength = len
            };

            archetype.world.tempArrays.Add(data);
            
            return chunk;
        }
    }
}
#endif