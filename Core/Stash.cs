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
    public abstract class Stash : IDisposable {
        [PublicAPI]
        public bool IsDisposed;

        internal long typeId;
        internal int offset;
        internal World world;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Set(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Remove(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void RemoveAll();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract bool Clean(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Migrate(Entity from, Entity to, bool overwrite = true);

        public abstract void Dispose();
    }

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Stash<T> : Stash where T : struct, IComponent {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal delegate void ComponentDispose(ref T component);
#endif

        internal T empty;
        internal IntHashMap<T> components;

#if !MORPEH_DISABLE_COMPONENT_DISPOSE
        internal ComponentDispose componentDispose;
#endif
        
        [UnityEngine.Scripting.Preserve]
        internal Stash() : this(-1) { }

        [UnityEngine.Scripting.Preserve]
        public Stash(int capacity = -1) {
            var info = TypeIdentifier<T>.info;
            
            this.typeId = info.id;
            this.offset = info.offset;

            this.components = new IntHashMap<T>(capacity < 0 ? info.stashSize : capacity);
        }

        internal Stash(Stash<T> other) {
            this.typeId = other.typeId;
            this.offset = other.offset;
            
            this.components = new IntHashMap<T>(other.components);
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            this.componentDispose = other.componentDispose;
#endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity");
            }

            var previousCapacity = this.components.capacity;
#endif
            if (this.components.Add(entity.entityId.id, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId, this.offset);
#if MORPEH_DEBUG
                if (previousCapacity != this.components.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return ref this.components.data[slotIndex];
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
            
            var previousCapacity = this.components.capacity;
#endif
            if (this.components.Add(entity.entityId.id, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId, this.offset);
                exist = false;
#if MORPEH_DEBUG
                if (previousCapacity != this.components.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                return ref this.components.data[slotIndex];
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
            
            var previousCapacity = this.components.capacity;
#endif
            if (this.components.Add(entity.entityId.id, value, out _)) {
                entity.AddTransfer(this.typeId, this.offset);
#if MORPEH_DEBUG
                if (previousCapacity != this.components.capacity) {
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

            if (!this.components.Has(entity.entityId.id)) {
                throw new Exception($"[MORPEH] You're trying to get on entity {entity.entityId.id} a component that doesn't exists!");
            }
#endif
            return ref this.components.GetValueRefByKey(entity.entityId.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(Entity entity, out bool exist) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity");
            }
#endif
            return ref this.components.TryGetValueRefByKey(entity.entityId.id, out exist);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Set(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity");
            }
            
            var previousCapacity = this.components.capacity;
#endif

            if (this.components.Set(entity.entityId.id, default, out _)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.components.capacity) {
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
            var previousCapacity = this.components.capacity;
#endif

            if (this.components.Set(entity.entityId.id, value, out _)) {
#if MORPEH_DEBUG
                if (previousCapacity != this.components.capacity) {
                    world.newMetrics.stashResizes++;
                }
#endif
                entity.AddTransfer(this.typeId, this.offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Remove(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Remove on null or disposed entity");
            }
#endif

            if (this.components.Remove(entity.entityId.id, out var lastValue)) {
                entity.RemoveTransfer(this.typeId, this.offset);
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
                this.componentDispose?.Invoke(ref lastValue);
#endif
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RemoveAll() {
            world.ThreadSafetyCheck();

#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            if (this.componentDispose != null) {
                foreach (var index in this.components) {
                    this.componentDispose.Invoke(ref this.components.data[index]);

                    var entityId = this.components.GetKeyByIndex(index);
                    this.world.GetEntity(entityId).RemoveTransfer(this.typeId, this.offset);
                }
            } 
            else 
#endif
            {
                foreach (var index in this.components) {
                    var entityId = this.components.GetKeyByIndex(index);
                    this.world.GetEntity(entityId).RemoveTransfer(this.typeId, this.offset);
                }
            }

            this.components.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool Clean(Entity entity) {
            if (this.components.Remove(entity.entityId.id, out var lastValue)) {
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
                this.componentDispose?.Invoke(ref lastValue);
#endif
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Has(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Has on null or disposed entity");
            }
#endif

            return this.components.Has(entity.entityId.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Migrate(Entity from, Entity to, bool overwrite = true) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Migrate FROM null or disposed entity");
            }
            if (to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Migrate TO null or disposed entity");
            }
            var previousCapacity = this.components.capacity;
#endif

            if (this.components.TryGetValue(from.entityId.id, out var component)) {
                if (overwrite) {
                    if (this.components.Has(to.entityId.id)) {
                        this.components.Set(to.entityId.id, component, out _);
                    }
                    else {
                        if (this.components.Add(to.entityId.id, component, out _)) {
                            to.AddTransfer(this.typeId, this.offset);
                        }
                    }
                }
                else {
                    if (this.components.Has(to.entityId.id) == false) {
                        if (this.components.Add(to.entityId.id, component, out _)) {
                            to.AddTransfer(this.typeId, this.offset);
                        }
                    }
                }

                if (this.components.Remove(from.entityId.id, out _)) {
                    from.RemoveTransfer(this.typeId, this.offset);
                }
            }
#if MORPEH_DEBUG
            if (previousCapacity != this.components.capacity) {
                world.newMetrics.stashResizes++;
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            world.ThreadSafetyCheck();
            
            return this.components.length == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty() {
            world.ThreadSafetyCheck();
            
            return this.components.length != 0;
        }

        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            
            world.ThreadSafetyCheck();
            
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            if (this.componentDispose != null) {
                foreach (var componentId in this.components) {
                    this.componentDispose.Invoke(ref this.components.data[componentId]);
                }
            }
#endif

            this.components.Clear();
            this.components = null;
            
#if !MORPEH_DISABLE_COMPONENT_DISPOSE
            this.componentDispose = null;
#endif
            this.IsDisposed = true;
        }
    }

}
