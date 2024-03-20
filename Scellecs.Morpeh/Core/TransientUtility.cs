namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    
    internal static class TransientUtility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize(ref TransientArchetype transient) {
            transient.nextArchetypeId = ArchetypeId.Invalid;
            
            transient.addedComponents = new IntHashMap<TypeInfo>(4);
            transient.removedComponents = new IntHashMap<TypeInfo>(4);
            
            transient.baseArchetype = null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rebase(ref TransientArchetype transient, Archetype archetype) {
            transient.nextArchetypeId = archetype != null ? archetype.id : ArchetypeId.Invalid;
            
            transient.addedComponents.Clear();
            transient.removedComponents.Clear();
            
            transient.baseArchetype = archetype;
            
            MLogger.LogTrace($"[Rebase] migration rebase to {transient.nextArchetypeId}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponent(ref TransientArchetype transient, ref TypeInfo typeInfo) {
            if (transient.removedComponents.Remove(typeInfo.offset.GetValue(), out _)) {
                MLogger.LogTrace($"[AddComponent] {typeInfo} migration canceled as it was removed before");
            } else {
                MLogger.LogTrace($"[AddComponent] {typeInfo} migration added from {transient.nextArchetypeId}");
                transient.addedComponents.Set(typeInfo.offset.GetValue(), typeInfo, out _);
            }

            transient.nextArchetypeId = transient.nextArchetypeId.Combine(ref typeInfo.id);
            MLogger.LogTrace($"[AddComponent] To: {transient.nextArchetypeId}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(ref TransientArchetype transient, ref TypeInfo typeInfo) {
            if (transient.addedComponents.Remove(typeInfo.offset.GetValue(), out _)) {
                MLogger.LogTrace($"[RemoveComponent] {typeInfo} migration canceled as it was added before");
            } else {
                MLogger.LogTrace($"[RemoveComponent] {typeInfo} migration added from {transient.nextArchetypeId}");
                transient.removedComponents.Set(typeInfo.offset.GetValue(), typeInfo, out _);
            }
            
            transient.nextArchetypeId = transient.nextArchetypeId.Combine(ref typeInfo.id);
            MLogger.LogTrace($"[RemoveComponent] To: {transient.nextArchetypeId}");
        }
    }
}