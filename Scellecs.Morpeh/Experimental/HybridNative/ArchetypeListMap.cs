#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

#if MORPEH_UNITY
using Scellecs.Morpeh;
using Scellecs.Morpeh.Experimental;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Assertions;

namespace Unity.Entities {
    [StructLayout(LayoutKind.Sequential)]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal unsafe struct ArchetypeListMap : IDisposable
    {
        private NativeHashMap<ArchetypeHash, int> archetypeHashToIndex;
        private UnsafePtrList<NativeArchetype> archetypesList;

        public int Count => archetypesList.Length;

        public bool IsEmpty => Count == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(int initialCapacity = 16)
        {
            archetypeHashToIndex = new NativeHashMap<ArchetypeHash, int>(initialCapacity, Allocator.Persistent);
            archetypesList = new UnsafePtrList<NativeArchetype>(initialCapacity, Allocator.Persistent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArchetype* TryGet(ArchetypeHash archetypeHash)
        {
            if (archetypeHashToIndex.TryGetValue(archetypeHash, out int index))
            {
                return archetypesList.Ptr[index];
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArchetype* Get(ArchetypeHash archetypeHash)
        {
            var result = TryGet(archetypeHash);
            Assert.IsFalse(result == null);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(NativeArchetype* archetype)
        {
            ArchetypeHash hash = archetype->hash;

            if (archetypeHashToIndex.ContainsKey(hash))
            {
                return;
            }

            // Add to list and store index in hashmap
            int index = archetypesList.Length;
            archetypesList.Add(archetype);
            archetypeHashToIndex.Add(hash, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(ArchetypeHash archetypeHash)
        {
            if (archetypeHashToIndex.TryGetValue(archetypeHash, out int indexToRemove))
            {
                int lastIndex = archetypesList.Length - 1;

                if (indexToRemove != lastIndex)
                {
                    NativeArchetype* lastArchetype = archetypesList.Ptr[lastIndex];
                    archetypesList.Ptr[indexToRemove] = lastArchetype;

                    archetypeHashToIndex[lastArchetype->hash] = indexToRemove;
                }

                archetypesList.RemoveAtSwapBack(lastIndex);
                archetypeHashToIndex.Remove(archetypeHash);

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(NativeArchetype* archetype)
        {
            ArchetypeHash hash = archetype->hash;
            if (archetypeHashToIndex.TryGetValue(hash, out int index))
            {
                if (archetypesList.Ptr[index] == archetype)
                {
                    return index;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            archetypeHashToIndex.Clear();
            archetypesList.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            archetypesList.SetCapacity(capacity);
        }

        public void Dispose()
        {
            archetypeHashToIndex.Dispose();
            archetypesList.Dispose();
        }
    }
}
#endif