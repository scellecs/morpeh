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
    
    public interface IStash : IDisposable { 
        public TypeHash TypeHash { get; }
        public int Length { get; }
        
        public void Set(Entity entity);
        public bool Remove(Entity entity);
        public void RemoveAll();
        public void Migrate(Entity from, Entity to, bool overwrite = true);
        public bool Has(Entity entity);
        internal void Clean(Entity entity);
    }

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Stash<T> : IStash where T : struct, IComponent {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal delegate void ComponentDispose(ref T component);
#endif

        internal World world;
        private TypeInfo typeInfo;
        
        internal readonly StashMap map;
        public T[] data;
        private T empty;

#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal ComponentDispose componentDispose;
#endif
        
        [PublicAPI]
        public bool IsDisposed;
        
        [PublicAPI]
        public TypeHash TypeHash => this.typeInfo.hash;
        
        [PublicAPI]
        public int Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.map.length;
        }

        [UnityEngine.Scripting.Preserve]
        internal Stash(World world, TypeInfo typeInfo, int capacity = -1) {
            this.world = world;
            this.typeInfo = typeInfo;
            
            this.map = new StashMap(capacity < 0 ? Constants.DEFAULT_STASH_COMPONENTS_CAPACITY : capacity);
            this.data = new T[this.map.capacity];
            
            this.empty = default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity}");
            }

            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.Id, default, out var slotIndex)) {
                world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return ref this.data[slotIndex];
            }
#if MORPEH_DEBUG
            MLogger.LogError($"You're trying to add on entity {entity} a component that already exists! Use Get or Set instead!");
#endif
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity, out bool exist) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity}");
            }
            
            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.Id, default, out var slotIndex)) {
                world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
                exist = false;
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return ref this.data[slotIndex];
            }

            exist = true;
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(Entity entity, in T value) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity}");
            }
            
            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.Id, value, out _)) {
                world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return true;
            }

#if MORPEH_DEBUG
            MLogger.LogError($"You're trying to add on entity {entity} a component that already exists! Use Get or Set instead!");
#endif
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity {entity}");
            }

            if (!this.map.Has(entity.Id)) {
                throw new Exception($"[MORPEH] You're trying to get on entity {entity} a component that doesn't exists!");
            }
#endif
            if (this.map.TryGetIndex(entity.Id, out var dataIndex)) {
                return ref this.data[dataIndex];
            }
            
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity, out bool exist) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity {entity}");
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
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity {entity}");
            }
            
            var previousCapacity = this.map.capacity;
#endif

            if (this.TrySetData(entity.Id, default)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity, in T value) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity {entity}");
            }
            var previousCapacity = this.map.capacity;
#endif

            if (this.TrySetData(entity.Id, value)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                world.TransientChangeAddComponent(entity.Id, ref this.typeInfo);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Remove on null or disposed entity {entity}");
            }
#endif

            if (this.map.Remove(entity.Id, out var slotIndex)) {
                world.TransientChangeRemoveComponent(entity.Id, ref this.typeInfo);
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
                this.componentDispose?.Invoke(ref this.data[slotIndex]);
#endif
                return true;
            }
            
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAll() {
            world.ThreadSafetyCheck();

#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            if (this.componentDispose != null) {
                foreach (var slotIndex in this.map) {
                    this.componentDispose.Invoke(ref this.data[slotIndex]);

                    var entityId = this.map.GetKeyBySlotIndex(slotIndex);
                    this.world.TransientChangeRemoveComponent(entityId, ref this.typeInfo);
                }
            } 
            else 
#endif
            {
                foreach (var slotIndex in this.map) {
                    var entityId = this.map.GetKeyBySlotIndex(slotIndex);
                    this.world.TransientChangeRemoveComponent(entityId, ref this.typeInfo);
                }
            }
            
            Array.Clear(this.data, 0, this.map.capacity);
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
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(from)) {
                throw new Exception($"[MORPEH] You are trying Migrate FROM null or disposed entity {from}");
            }
            if (world.IsDisposed(to)) {
                throw new Exception($"[MORPEH] You are trying Migrate TO null or disposed entity {to}");
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
                    this.world.TransientChangeRemoveComponent(from.Id, ref this.typeInfo);
                }
            }
#if MORPEH_DEBUG
            if (previousCapacity != this.map.capacity) {
                world.newMetrics.stashResizes++;
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.IsDisposed(entity)) {
                throw new Exception($"[MORPEH] You are trying Has on null or disposed entity {entity}");
            }
#endif

            return this.map.Has(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            world.ThreadSafetyCheck();
            
            return this.map.length == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty() {
            world.ThreadSafetyCheck();
            
            return this.map.length != 0;
        }

        public void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            
            world.ThreadSafetyCheck();
            
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            if (this.componentDispose != null) {
                foreach (var slotIndex in this.map) {
                    this.componentDispose.Invoke(ref this.data[slotIndex]);
                }
            }
#endif

            if (!this.map.IsEmpty()) {
                Array.Clear(this.data, 0, this.map.capacity);
                this.map.Clear();
            }
            
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
