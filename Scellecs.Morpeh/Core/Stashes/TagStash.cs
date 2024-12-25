#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    // TODO: AsNative support
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TagStash : IStash {
        internal World    world;
        private  Type     type;
        private  TypeInfo typeInfo;
        
        private IntHashSet set;
        
        // TODO: Remove TRUE after migrating functionality to the new API
#if UNITY_EDITOR || MORPEH_ENABLE_RUNTIME_BOXING_API || TRUE
        private IComponent boxedValue;
#endif
        
        [PublicAPI]
        public bool IsDisposed;

        [PublicAPI]
        public Type Type {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.type;
        }
        
        [PublicAPI]
        public int Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.set.length;
        }
        
        [UnityEngine.Scripting.Preserve]
        internal TagStash(World world, Type type, TypeInfo typeInfo, int capacity = -1) {
            this.world = world;
            this.type = type;
            this.typeInfo = typeInfo;
            
            this.set = new IntHashSet(capacity < 0 ? StashConstants.DEFAULT_COMPONENTS_CAPACITY : capacity);
            
            // TODO: Remove TRUE after migrating functionality to the new API
#if UNITY_EDITOR || MORPEH_ENABLE_RUNTIME_BOXING_API || TRUE
            this.boxedValue = Activator.CreateInstance(type) as IComponent;
#endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.set.Add(entity.Id)) {
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            } else {
                InvalidAddOperationException.ThrowAlreadyExists(entity, this.type);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.set.Add(entity.Id)) {
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidRemoveOperationException.ThrowDisposedEntity(entity, this.type);
            }

            if (!this.set.Remove(entity.Id)) {
                return false;
            }

            this.world.TransientChangeRemoveComponent(entity.Id, ref this.typeInfo);
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAll() {
            this.world.ThreadSafetyCheck();
            
            foreach (var entityId in this.set) {
                this.world.TransientChangeRemoveComponent(entityId, ref this.typeInfo);
            }
            
            this.set.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IStash.Clean(Entity entity) {
            this.set.Remove(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Migrate(Entity from, Entity to, bool overwrite = true) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(from)) {
                InvalidMigrateOperationException.ThrowDisposedEntityFrom(from, this.type);
            }
            
            if (this.world.IsDisposed(to)) {
                InvalidMigrateOperationException.ThrowDisposedEntityTo(to, this.type);
            }

            if (!this.set.Has(from.Id)) {
                return;
            }

            if (this.set.Add(to.Id)) {
                this.world.TransientChangeAddComponent(to.Id, ref this.typeInfo);
            }
                
            if (this.set.Remove(from.Id)) {
                this.world.TransientChangeRemoveComponent(from.Id, ref this.typeInfo);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidHasOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            return this.set.Has(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            this.world.ThreadSafetyCheck();
            
            return this.set.length == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty() {
            this.world.ThreadSafetyCheck();
            
            return this.set.length != 0;
        }
        
        // TODO: Remove TRUE after migrating functionality to the new API
#if UNITY_EDITOR || MORPEH_ENABLE_RUNTIME_BOXING_API || TRUE
        public IComponent GetBoxed(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.set.Has(entity.Id)) {
                return this.boxedValue;
            }
            
            InvalidGetOperationException.ThrowMissing(entity, this.type);
            return null;
        }

        public IComponent GetBoxed(Entity entity, out bool exists) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            exists = this.set.Has(entity.Id);
            return exists ? this.boxedValue : null;
        }

        public void SetBoxed(Entity entity, IComponent value) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, this.type);
            }

            if (this.set.Add(entity.Id)) {
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }
#endif
        
        public void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            
            this.world.ThreadSafetyCheck();
            
            this.world = null;
            this.typeInfo = default;
            
            this.set = null;
            
            this.IsDisposed = true;
        }
    }
}
