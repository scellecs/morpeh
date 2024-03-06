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
    public class Stash : IDisposable {
        internal StashMap stashMap;
        internal IDisposable typelessStash;
        
        internal long typeId;
        
        private Action<Entity> setReflection;
        private Func<Entity, bool> removeReflection;
        private Func<Entity, bool> cleanReflection;
        private Action removeAllReflection;
        private Action<Entity, Entity, bool> migrateReflection;
        
        private Stash() { }

        public static Stash CreateReflection(World world, Type type) {
            var createMethod = typeof(Stash).GetMethod("Create", new[] { typeof(World), });
            var genericMethod = createMethod?.MakeGenericMethod(type);
            return (Stash)genericMethod?.Invoke(null, new object[] { world, });
        }

        public static Stash Create<T>(World world) where T : struct, IComponent {
            var info = TypeIdentifier<T>.info;
            var stash = new Stash<T>(world, info);
            
            return new Stash {
                stashMap = stash.map,
                typelessStash = stash,
                
                typeId = info.id,
                
                setReflection = stash.Set,
                removeReflection = stash.Remove,
                cleanReflection = stash.Clean,
                removeAllReflection = stash.RemoveAll,
                migrateReflection = stash.Migrate,
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity) {
            this.setReflection(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            return this.removeReflection(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAll() {
            this.removeAllReflection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Clean(Entity entity) {
            return this.cleanReflection(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Migrate(Entity from, Entity to, bool overwrite = true) {
            this.migrateReflection(from, to, overwrite);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            return this.stashMap.Has(entity.entityId.id);
        }

        public void Dispose() {
            this.typelessStash.Dispose();
        }
    }

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Stash<T> : IDisposable where T : struct, IComponent {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal delegate void ComponentDispose(ref T component);
#endif
        [PublicAPI]
        public bool IsDisposed;

        private World world;
        private long typeId;
        private int offset;
        
        internal readonly StashMap map;
        public T[] data;
        private T empty;

#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal ComponentDispose componentDispose;
#endif

        [UnityEngine.Scripting.Preserve]
        internal Stash(World world, CommonTypeIdentifier.TypeInfo typeInfo, int capacity = -1) {
            this.world = world;
            this.typeId = typeInfo.id;
            this.offset = typeInfo.offset;
            
            this.map = new StashMap(capacity < 0 ? typeInfo.stashSize : capacity);
            this.data = new T[this.map.capacity];
            
            this.empty = default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity");
            }

            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.entityId.id, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId, this.offset);
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return ref this.data[slotIndex];
            }
#if MORPEH_DEBUG
            MLogger.LogError($"You're trying to add on entity {entity.entityId.id} a component that already exists! Use Get or Set instead!");
#endif
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity, out bool exist) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity");
            }
            
            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.entityId.id, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId, this.offset);
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
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity");
            }
            
            var previousCapacity = this.map.capacity;
#endif
            if (this.TryAddData(entity.entityId.id, value, out _)) {
                entity.AddTransfer(this.typeId, this.offset);
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return true;
            }

#if MORPEH_DEBUG
            MLogger.LogError($"You're trying to add on entity {entity.entityId.id} a component that already exists! Use Get or Set instead!");
#endif
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity");
            }

            if (!this.map.Has(entity.entityId.id)) {
                throw new Exception($"[MORPEH] You're trying to get on entity {entity.entityId.id} a component that doesn't exists!");
            }
#endif
            if (this.map.TryGetIndex(in entity.entityId.id, out var dataIndex)) {
                return ref this.data[dataIndex];
            }
            
            return ref this.empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity, out bool exist) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity");
            }
#endif
            if (this.map.TryGetIndex(in entity.entityId.id, out var dataIndex))
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
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity");
            }
            
            var previousCapacity = this.map.capacity;
#endif

            if (this.TrySetData(entity.entityId.id, default)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                entity.AddTransfer(this.typeId, this.offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity, in T value) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity");
            }
            var previousCapacity = this.map.capacity;
#endif

            if (this.TrySetData(entity.entityId.id, value)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.map.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                entity.AddTransfer(this.typeId, this.offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Remove on null or disposed entity");
            }
#endif

            if (this.map.Remove(entity.entityId.id, out var slotIndex)) {
                entity.RemoveTransfer(this.typeId, this.offset);
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
                foreach (var index in this) {
                    this.componentDispose.Invoke(ref this.data[index]);

                    var entityId = this.map.GetKeyByIndex(index);
                    this.world.GetEntity(entityId).RemoveTransfer(this.typeId, this.offset);
                }
            } 
            else 
#endif
            {
                foreach (var index in this) {
                    var entityId = this.map.GetKeyByIndex(index);
                    this.world.GetEntity(entityId).RemoveTransfer(this.typeId, this.offset);
                }
            }

            if (!this.map.IsEmpty()) {
                Array.Clear(this.data, 0, this.map.capacity);
                this.map.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Clean(Entity entity) {
            if (this.map.Remove(entity.entityId.id, out var slotIndex)) {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
                this.componentDispose?.Invoke(ref this.data[slotIndex]);
#endif
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Has on null or disposed entity");
            }
#endif

            return this.map.Has(entity.entityId.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Migrate(Entity from, Entity to, bool overwrite = true) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Migrate FROM null or disposed entity");
            }
            if (to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Migrate TO null or disposed entity");
            }
            var previousCapacity = this.map.capacity;
#endif

            if (this.map.TryGetIndex(in from.entityId.id, out var dataIndex)) {
                var component = this.data[dataIndex];
                
                if (overwrite) {
                    if (this.map.Has(to.entityId.id)) {
                        this.TrySetData(to.entityId.id, component);
                    }
                    else {
                        if (this.TryAddData(to.entityId.id, component, out _)) {
                            to.AddTransfer(this.typeId, this.offset);
                        }
                    }
                }
                else {
                    if (this.map.Has(to.entityId.id) == false) {
                        if (this.TryAddData(to.entityId.id, component, out _)) {
                            to.AddTransfer(this.typeId, this.offset);
                        }
                    }
                }

                if (this.map.Remove(from.entityId.id, out _)) {
                    from.RemoveTransfer(this.typeId, this.offset);
                }
            }
#if MORPEH_DEBUG
            if (previousCapacity != this.map.capacity) {
                world.newMetrics.stashResizes++;
            }
#endif
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
                foreach (var componentId in this) {
                    this.componentDispose.Invoke(ref this.data[componentId]);
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
            Enumerator e;
            e.stash = this;
            e.index   = 0;
            e.current = default;
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Enumerator {
            public Stash<T> stash;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < this.stash.map.lastIndex; ++this.index) {
                    ref var slot = ref this.stash.map.slots.ptr[this.index];
                    if (slot.key - 1 < 0) {
                        continue;
                    }

                    this.current = this.index;
                    ++this.index;

                    return true;
                }

                this.index   = this.stash.map.lastIndex + 1;
                this.current = default;
                return false;
            }

            public int Current => this.current;
        }
    }

}
