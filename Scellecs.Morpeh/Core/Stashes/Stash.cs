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
    public sealed class Stash<T> : IStash where T : struct, IComponent {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal delegate void ComponentDispose(ref T component);
        internal ComponentDispose componentDispose;
#endif
        
        internal World world;
        private TypeInfo typeInfo;
        
        
        internal IntSlotMap map;
        public T[] data;
        internal T empty;
        private Type type;
        
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
        internal Stash(World world, TypeInfo typeInfo, int capacity = -1) {
            this.world = world;
            this.typeInfo = typeInfo;
            
            this.map = new IntSlotMap(capacity < 0 ? StashConstants.DEFAULT_COMPONENTS_CAPACITY : capacity);
            this.data = new T[this.map.capacity];
            
            this.empty = default;
            
            this.type = typeof(T);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.map.IsKeySet(entity.Id, out var slotIndex)) {
                InvalidAddOperationException.ThrowAlreadyExists(entity, this.type);
            } else {
                slotIndex = this.map.TakeSlot(entity.Id, out var resized);
                
                if (resized) {
                    ArrayHelpers.GrowNonInlined(ref this.data, this.map.capacity);
#if MORPEH_DEBUG
                    this.world.newMetrics.stashResizes++;
#endif
                }
                
                this.data[slotIndex] = default;
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
            
            return ref this.data[slotIndex];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity, out bool exist) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (!this.map.IsKeySet(entity.Id, out var slotIndex)) {
                slotIndex = this.map.TakeSlot(entity.Id, out var resized);
                
                if (resized) {
                    ArrayHelpers.GrowNonInlined(ref this.data, this.map.capacity);
#if MORPEH_DEBUG
                    this.world.newMetrics.stashResizes++;
#endif
                }
                
                this.data[slotIndex] = default;
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
                
                exist = false;
                return ref this.data[slotIndex];
            }
            
            exist = true;
            return ref this.empty;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Entity entity, in T value) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.map.IsKeySet(entity.Id, out var slotIndex)) {
                InvalidAddOperationException.ThrowAlreadyExists(entity, this.type);
            } else {
                slotIndex = this.map.TakeSlot(entity.Id, out var resized);
                
                if (resized) {
                    ArrayHelpers.GrowNonInlined(ref this.data, this.map.capacity);
#if MORPEH_DEBUG
                    this.world.newMetrics.stashResizes++;
#endif
                }
                
                this.data[slotIndex] = value;
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.map.TryGetIndex(entity.Id, out var dataIndex)) {
                return ref this.data[dataIndex];
            }
            
            InvalidGetOperationException.ThrowMissing(entity, this.type);
            return ref this.empty;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity, out bool exist) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.map.TryGetIndex(entity.Id, out var dataIndex)) {
                exist = true;
                return ref this.data[dataIndex];
            }
            
            exist = false;
            return ref this.empty;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (!this.map.IsKeySet(entity.Id, out var slotIndex)) {
                slotIndex = this.map.TakeSlot(entity.Id, out var resized);
                
                if (resized) {
                    ArrayHelpers.GrowNonInlined(ref this.data, this.map.capacity);
#if MORPEH_DEBUG
                    this.world.newMetrics.stashResizes++;
#endif
                }
                
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
            
            this.data[slotIndex] = default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity, in T value) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (!this.map.IsKeySet(entity.Id, out var slotIndex)) {
                slotIndex = this.map.TakeSlot(entity.Id, out var resized);
                
                if (resized) {
                    ArrayHelpers.GrowNonInlined(ref this.data, this.map.capacity);
#if MORPEH_DEBUG
                    this.world.newMetrics.stashResizes++;
#endif
                }
                
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
            
            this.data[slotIndex] = value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            this.world.ThreadSafetyCheck();
            
            if (this.world.IsDisposed(entity)) {
                InvalidRemoveOperationException.ThrowDisposedEntity(entity, this.type);
            }
            
            if (this.map.Remove(entity.Id, out var slotIndex)) {
                this.world.TransientChangeRemoveComponent(entity.Id, ref this.typeInfo);
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
                this.componentDispose?.Invoke(ref this.data[slotIndex]);
#endif
                this.data[slotIndex] = default;
                return true;
            }
            
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAll() {
            this.world.ThreadSafetyCheck();
            
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            if (this.componentDispose != null) {
                foreach (var slotIndex in this.map) {
                    this.componentDispose.Invoke(ref this.data[slotIndex]);
                    this.data[slotIndex] = default;
                    
                    var entityId = this.map.GetKeyBySlotIndex(slotIndex);
                    this.world.TransientChangeRemoveComponent(entityId, ref this.typeInfo);
                }
            } 
            else 
#endif
            {
                foreach (var slotIndex in this.map) {
                    this.data[slotIndex] = default;
                    
                    var entityId = this.map.GetKeyBySlotIndex(slotIndex);
                    this.world.TransientChangeRemoveComponent(entityId, ref this.typeInfo);
                }
            }
            
            this.map.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IStash.Clean(Entity entity) {
            if (this.map.Remove(entity.Id, out var slotIndex)) {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
                this.componentDispose?.Invoke(ref this.data[slotIndex]);
#endif
                this.data[slotIndex] = default;
            }
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
            
            if (this.map.TryGetIndex(from.Id, out var fromSlotIndex)) {
                ref var component = ref this.data[fromSlotIndex];
                
                if (!this.map.IsKeySet(to.Id, out var toSlotIndex)) {
                    toSlotIndex = this.map.TakeSlot(to.Id, out var resized);
                    
                    if (resized) {
                        ArrayHelpers.GrowNonInlined(ref this.data, this.map.capacity);
#if MORPEH_DEBUG
                        this.world.newMetrics.stashResizes++;
#endif
                    }
                    
                    this.data[toSlotIndex] = component;
                    this.world.TransientChangeAddComponent(to.Id, ref this.typeInfo);
                } else if (overwrite) {
                    this.data[toSlotIndex] = component;
                }
                
                if (this.map.Remove(from.Id, out _)) {
                    this.data[fromSlotIndex] = default;
                    this.world.TransientChangeRemoveComponent(from.Id, ref this.typeInfo);
                }
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
            
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            if (this.componentDispose != null) {
                foreach (var slotIndex in this.map) {
                    this.componentDispose.Invoke(ref this.data[slotIndex]);
                    this.data[slotIndex] = default;
                }
            }
            else
#endif
            {
                foreach (var slotIndex in this.map) {
                    this.data[slotIndex] = default;
                }
            }
            this.world = null;
            this.typeInfo = default;
            
            
            this.map.Dispose();
            this.map = null;
            this.data = null;
            this.empty = default;
            
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            this.componentDispose = null;
#endif
            this.IsDisposed = true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.mapEnumerator = this.map.GetEnumerator();
            e.data = this.data;
            return e;
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            internal IntSlotMap.Enumerator mapEnumerator;
            internal T[] data;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => this.mapEnumerator.MoveNext();
            
            public ref T Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref this.data[this.mapEnumerator.Current];
            }
        }
    }
}
