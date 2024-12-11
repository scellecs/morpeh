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

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TagStash : IStash {
        internal World    world;
        private  Type     type;
        private  TypeInfo typeInfo;
        
        private IntHashSet map;
        
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
            get => this.map.length;
        }
        
        [UnityEngine.Scripting.Preserve]
        internal TagStash(World world, Type type, TypeInfo typeInfo, int capacity = -1) {
            this.world = world;
            this.type = type;
            this.typeInfo = typeInfo;
            
            this.map = new IntHashSet(capacity < 0 ? StashConstants.DEFAULT_COMPONENTS_CAPACITY : capacity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.map.Has(entity.Id)) {
                InvalidAddOperationException.ThrowAlreadyExists(entity, this.type);
            } else {
                this.map.Add(entity.Id);
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (!this.map.Has(entity.Id)) {
                this.map.Add(entity.Id);
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidRemoveOperationException.ThrowDisposedEntity(entity, this.type);
            }

            if (!this.map.Remove(entity.Id)) {
                return false;
            }

            this.world.TransientChangeRemoveComponent(entity.Id, ref this.typeInfo);
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAll() {
            this.world.ThreadSafetyCheck();
            
            foreach (var entityId in this.map) {
                this.world.TransientChangeRemoveComponent(entityId, ref this.typeInfo);
            }
            
            this.map.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IStash.Clean(Entity entity) {
            this.map.Remove(entity.Id);
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

            if (!this.map.Has(from.Id)) {
                return;
            }

            if (!this.map.Has(to.Id)) {
                this.map.Add(to.Id);
                this.world.TransientChangeAddComponent(to.Id, ref this.typeInfo);
            }
                
            if (this.map.Remove(from.Id)) {
                this.world.TransientChangeRemoveComponent(from.Id, ref this.typeInfo);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidHasOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            return this.map.Has(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            this.world.ThreadSafetyCheck();
            
            return this.map.length == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty() {
            this.world.ThreadSafetyCheck();
            
            return this.map.length != 0;
        }
        
        public void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            
            this.world.ThreadSafetyCheck();
            
            this.world = null;
            this.typeInfo = default;
            
            this.map = null;
            
            this.IsDisposed = true;
        }
    }
}
