namespace Scellecs.Morpeh {
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    using Collections;
    
    // TODO: Make it a struct
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal class TransientArchetype {
        internal Archetype baseArchetype;
        
        internal HashSet<TypeInfo> addedComponents = new HashSet<TypeInfo>(4);
        internal HashSet<TypeInfo> removedComponents = new HashSet<TypeInfo>(4);
        
        internal ArchetypeId nextArchetypeId;

        public bool IsEmpty {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.addedComponents.Count + this.removedComponents.Count == 0;
        }
        
        public void Rebase(Archetype archetype) {
            MLogger.LogTrace($"[Rebase] migration rebase");
            this.baseArchetype = archetype;
            this.nextArchetypeId = this.baseArchetype == null ? ArchetypeId.Invalid : archetype.id;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() {
            MLogger.LogTrace($"[Reset] migration reset");
            this.baseArchetype = null;
            this.nextArchetypeId = ArchetypeId.Invalid;
            this.addedComponents.Clear();
            this.removedComponents.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SoftReset() {
            MLogger.LogTrace($"[SoftReset] migration soft reset");
            this.baseArchetype = null;
            this.nextArchetypeId = ArchetypeId.Invalid;
        }
        
        public void AddComponent(TypeInfo typeInfo) {
            if (this.removedComponents.Remove(typeInfo)) {
                MLogger.LogTrace($"[AddComponent] {typeInfo} migration canceled as it was removed before");
            } else {
                if (this.baseArchetype != null && this.baseArchetype.components.Get(typeInfo.offset.GetValue())) {
                    return;
                }
                
                MLogger.LogTrace($"[AddComponent] {typeInfo} migration added {typeInfo.GetHashCode()}");
                this.addedComponents.Add(typeInfo);
            }
            
            this.nextArchetypeId = this.nextArchetypeId.Combine(typeInfo.id);
        }
        
        public void RemoveComponent(TypeInfo typeInfo) {
            if (this.addedComponents.Remove(typeInfo)) {
                MLogger.LogTrace($"[RemoveComponent] {typeInfo} migration canceled as it was added before");
            } else {
                if (this.baseArchetype != null && !this.baseArchetype.components.Get(typeInfo.offset.GetValue())) {
                    return;
                }
                
                MLogger.LogTrace($"[RemoveComponent] {typeInfo} migration added {typeInfo.GetHashCode()}");
                this.removedComponents.Add(typeInfo);
            }
            
            this.nextArchetypeId = this.nextArchetypeId.Combine(typeInfo.id);
        }
    }
}