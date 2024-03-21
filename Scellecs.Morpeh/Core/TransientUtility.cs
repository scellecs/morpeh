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
            if (transient.changesCount == transient.changes.Length) {
                Array.Resize(ref transient.changes, transient.changesCount << 1);
            }
            
            transient.changes[transient.changesCount] = StructuralChange.Create(typeInfo.offset, true);
            ++transient.changesCount;
            transient.nextArchetypeId = transient.nextArchetypeId.Combine(typeInfo.id);
            MLogger.LogTrace($"[AddComponent] To: {transient.nextArchetypeId}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(ref TransientArchetype transient, ref TypeInfo typeInfo) {
            if (transient.changesCount == transient.changes.Length) {
                Array.Resize(ref transient.changes, transient.changesCount << 1);
            }
            
            transient.changes[transient.changesCount] = StructuralChange.Create(typeInfo.offset, false);
            ++transient.changesCount;
            transient.nextArchetypeId = transient.nextArchetypeId.Combine(typeInfo.id);
            MLogger.LogTrace($"[RemoveComponent] To: {transient.nextArchetypeId}");
        }
    }
}