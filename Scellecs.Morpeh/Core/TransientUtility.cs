using System;

namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    using Scellecs.Morpeh.Collections;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class TransientUtility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize(ref TransientArchetype transient) {
            transient.nextArchetypeId = ArchetypeId.Invalid;
            transient.changes = new StructuralChange[16];
            transient.changesCount = 0;
            transient.baseArchetype = null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rebase(ref TransientArchetype transient, Archetype archetype) {
            transient.nextArchetypeId = archetype != null ? archetype.id : ArchetypeId.Invalid;
            transient.changesCount = 0;
            transient.baseArchetype = archetype;
            
            MLogger.LogTrace($"[Rebase] migration rebase to {transient.nextArchetypeId}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponent(ref TransientArchetype transient, ref TypeInfo typeInfo) {
            if (!FilterDuplicate(ref transient, typeInfo.offset)) {
                EnsureCapacity(ref transient);
                transient.changes[transient.changesCount++] = StructuralChange.Create(typeInfo.offset, true);
            }
            
            transient.nextArchetypeId = transient.nextArchetypeId.Combine(typeInfo.id);
            MLogger.LogTrace($"[AddComponent] To: {transient.nextArchetypeId}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(ref TransientArchetype transient, ref TypeInfo typeInfo) {
            if (!FilterDuplicate(ref transient, typeInfo.offset)) {
                EnsureCapacity(ref transient);
                transient.changes[transient.changesCount++] = StructuralChange.Create(typeInfo.offset, false);
            }
            
            transient.nextArchetypeId = transient.nextArchetypeId.Combine(typeInfo.id);
            MLogger.LogTrace($"[RemoveComponent] To: {transient.nextArchetypeId}");
        }
        
        // We use array filtering with swap because we don't expect a lot of changes in one frame for one entity
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool FilterDuplicate(ref TransientArchetype transient, TypeOffset typeOffset) {
            var changesCount = transient.changesCount;
            
            for (var i = 0; i < changesCount; i++) {
                if (transient.changes[i].typeOffset != typeOffset) {
                    continue;
                }
                
                RemoveAtSwap(ref transient, i);
                return true;
            }
            
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RemoveAtSwap(ref TransientArchetype transient, int index) {
            transient.changes[index] = transient.changes[transient.changesCount - 1];
            --transient.changesCount;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity(ref TransientArchetype transient) {
            if (transient.changesCount == transient.changes.Length) {
                Array.Resize(ref transient.changes, transient.changesCount << 1);
            }
        }
    }
}