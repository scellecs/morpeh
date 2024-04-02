using System;

namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class EntityDataUtility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponent(ref EntityData entityData, ref TypeInfo typeInfo) {
            if (!FilterDuplicate(ref entityData, typeInfo.offset)) {
                EnsureCapacity(ref entityData);
                entityData.changes[entityData.changesCount++] = StructuralChange.Create(typeInfo.offset, true);
            }
            
            entityData.nextArchetypeHash = entityData.nextArchetypeHash.Combine(typeInfo.id);
            MLogger.LogTrace($"[AddComponent] To: {entityData.nextArchetypeHash}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(ref EntityData entityData, ref TypeInfo typeInfo) {
            if (!FilterDuplicate(ref entityData, typeInfo.offset)) {
                EnsureCapacity(ref entityData);
                entityData.changes[entityData.changesCount++] = StructuralChange.Create(typeInfo.offset, false);
            }
            
            entityData.nextArchetypeHash = entityData.nextArchetypeHash.Combine(typeInfo.id);
            MLogger.LogTrace($"[RemoveComponent] To: {entityData.nextArchetypeHash}");
        }
        
        // We use array filtering with swap because we don't expect a lot of changes in one frame for one entity
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool FilterDuplicate(ref EntityData entityData, TypeOffset typeOffset) {
            var changesCount = entityData.changesCount;
            
            for (var i = 0; i < changesCount; i++) {
                if (entityData.changes[i].typeOffset != typeOffset) {
                    continue;
                }
                
                entityData.changes[i] = entityData.changes[entityData.changesCount - 1];
                --entityData.changesCount;
                
                return true;
            }
            
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity(ref EntityData entityData) {
            if (entityData.changesCount == entityData.changes.Length) {
                Array.Resize(ref entityData.changes, entityData.changesCount << 1);
            }
        }
    }
}