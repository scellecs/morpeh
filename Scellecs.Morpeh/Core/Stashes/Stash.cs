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

    [Il2CppEagerStaticClassConstruction]
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
        
        
        internal StashMap map;
        public T[] data;
        private T empty;
        
        [PublicAPI]
        public bool IsDisposed;
        
        [PublicAPI]
        public Type Type {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => typeof(T);
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
            
            this.map = new StashMap(typeInfo.stashSize);
            this.data = new T[this.map.capacity];
            
            this.empty = default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }

            var previousCapacity = this.map.capacity;
#endif
            if (!this.TryAddData(entity.Id, default, out var slotIndex)) {
                InvalidAddOperationException.ThrowAlreadyExists(entity, typeof(T));
            }
            
            this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
#if MORPEH_DEBUG
            if (previousCapacity != this.map.capacity) {
                this.world.newMetrics.stashResizes++;
            }
#endif
            return ref this.data[slotIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity, out bool exist) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
            
            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.Id, default, out var slotIndex)) {
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
                exist = false;
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    this.world.newMetrics.stashResizes++;
                }
#endif
                return ref this.data[slotIndex];
            }

            exist = true;
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(Entity entity, in T value) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
            
            var previousCapacity = this.map.capacity;
#endif
            if (!this.TryAddData(entity.Id, value, out _)) {
                InvalidAddOperationException.ThrowAlreadyExists(entity, typeof(T));
            }
            
            this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
#if MORPEH_DEBUG
            if (previousCapacity != this.map.capacity) {
                this.world.newMetrics.stashResizes++;
            }
#endif
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }

            if (!this.map.Has(entity.Id)) {
                InvalidGetOperationException.ThrowMissing(entity, typeof(T));
            }
#endif
            if (this.map.TryGetIndex(entity.Id, out var dataIndex)) {
                return ref this.data[dataIndex];
            }
            
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity, out bool exist) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            if (this.map.TryGetIndex(entity.Id, out var dataIndex))
            {
                exist = true;
                return ref this.data[dataIndex];
            }
            
            exist = false;
            return ref this.empty;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
            
            var previousCapacity = this.map.capacity;
#endif

            if (this.TrySetData(entity.Id, default)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    this.world.newMetrics.stashResizes++;
                }
#endif
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity, in T value) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
            var previousCapacity = this.map.capacity;
#endif

            if (this.TrySetData(entity.Id, value)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    this.world.newMetrics.stashResizes++;
                }
#endif
                this.world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidRemoveOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif

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
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(from)) {
                InvalidMigrateOperationException.ThrowDisposedEntityFrom(from, typeof(T));
            }
            
            if (this.world.IsDisposed(to)) {
                InvalidMigrateOperationException.ThrowDisposedEntityTo(to, typeof(T));
            }
            
            var previousCapacity = this.map.capacity;
#endif

            if (this.map.TryGetIndex(from.Id, out var slotIndex)) {
                var component = this.data[slotIndex];
                
                if (overwrite) {
                    if (this.map.Has(to.Id)) {
                        this.TrySetData(to.Id, component);
                    }
                    else {
                        if (this.TryAddData(to.Id, component, out _)) {
                            this.world.TransientChangeAddComponent(to.Id, ref this.typeInfo);
                        }
                    }
                }
                else {
                    if (this.map.Has(to.Id) == false) {
                        if (this.TryAddData(to.Id, component, out _)) {
                            this.world.TransientChangeAddComponent(to.Id, ref this.typeInfo);
                        }
                    }
                }

                if (this.map.Remove(from.Id, out _)) {
                    this.data[slotIndex] = default;
                    this.world.TransientChangeRemoveComponent(from.Id, ref this.typeInfo);
                }
            }
#if MORPEH_DEBUG
            if (previousCapacity != this.map.capacity) {
                this.world.newMetrics.stashResizes++;
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            this.world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (this.world.IsDisposed(entity)) {
                InvalidHasOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif

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
                }
            }
#endif
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
            return new Enumerator {
                mapEnumerator = this.map.GetEnumerator(),
                data = this.data,
            };
        }
            
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            internal StashMap.Enumerator mapEnumerator;
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
