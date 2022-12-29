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
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class Stash : IDisposable {
        internal static FastList<Stash> stashes = new FastList<Stash>();

        internal static Action cleanup = () => stashes.Clear();

        [SerializeField]
        internal int commonStashId;
        [SerializeField]
        internal int typedStashId;
        [SerializeField]
        internal int typeId;
        [SerializeField]
        internal World world;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Set(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Remove(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract bool Clean(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Migrate(Entity from, Entity to, bool overwrite = true);

        public abstract void Dispose();
    }

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Stash<T> : Stash where T : struct, IComponent {
        internal static FastList<Stash<T>> typedStashes = new FastList<Stash<T>>();

        internal delegate void ComponentDispose(ref T component);

        [SerializeField]
        internal IntHashMap<T> components;

        internal ComponentDispose componentDispose;

        [UnityEngine.Scripting.Preserve]
        static Stash() {
            cleanup += () => typedStashes.Clear();
        }

        [UnityEngine.Scripting.Preserve]
        static void Refill() {
            var id = TypeIdentifier<T>.info.id;
            if (typedStashes == null) {
                typedStashes = new FastList<Stash<T>>();
            }
            typedStashes.Clear();

            foreach (var stash in stashes) {
                if (stash.typeId == id) {
                    typedStashes.Add((Stash<T>)stash);
                }
            }
        }

        [UnityEngine.Scripting.Preserve]
        internal Stash() {
            var info = TypeIdentifier<T>.info;
            
            this.typeId = info.id;

            this.components = new IntHashMap<T>(info.stashSize);

            this.components.Add(-1, default, out _);

            this.typedStashId = typedStashes.length;
            typedStashes.Add(this);

            this.commonStashId = stashes.length;
            stashes.Add(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity.entityId.id}");
            }
#endif
            if (this.components.Add(entity.entityId.id, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId);
                return ref this.components.data[slotIndex];
            }
#if MORPEH_DEBUG
            MLogger.LogError($"You're trying to add on entity {entity.entityId.id} a component that already exists! Use Get or Set instead!");
#endif
            return ref this.components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(Entity entity, out bool exist) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity.entityId.id}");
            }
#endif
            if (this.components.Add(entity.entityId.id, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId);
                exist = false;
                return ref this.components.data[slotIndex];
            }

            exist = true;
            return ref this.components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(Entity entity, in T value) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Add on null or disposed entity {entity.entityId.id}");
            }
#endif
            if (this.components.Add(entity.entityId.id, value, out _)) {
                entity.AddTransfer(this.typeId);
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
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity {entity.entityId.id}");
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
                throw new Exception($"[MORPEH] You are trying Get on null or disposed entity {entity.entityId.id}");
            }
#endif
            return ref this.components.TryGetValueRefByKey(entity.entityId.id, out exist);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Set(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity {entity.entityId.id}");
            }
#endif

            if (this.components.Set(entity.entityId.id, default, out _)) {
                entity.AddTransfer(this.typeId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity, in T value) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Set on null or disposed entity {entity.entityId.id}");
            }
#endif

            if (this.components.Set(entity.entityId.id, value, out _)) {
                entity.AddTransfer(this.typeId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.components.data[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Remove(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Remove on null or disposed entity {entity.entityId.id}");
            }
#endif

            if (this.components.Remove(entity.entityId.id, out var lastValue)) {
                entity.RemoveTransfer(this.typeId);
                this.componentDispose?.Invoke(ref lastValue);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool Clean(Entity entity) {
            if (this.components.Remove(entity.entityId.id, out var lastValue)) {
                this.componentDispose?.Invoke(ref lastValue);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Has(Entity entity) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Has on null or disposed entity {entity.entityId.id}");
            }
#endif

            return this.components.Has(entity.entityId.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Migrate(Entity from, Entity to, bool overwrite = true) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Migrate FROM null or disposed entity {from.entityId.id}");
            }
            if (to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Migrate TO null or disposed entity {to.entityId.id}");
            }
#endif

            if (this.components.TryGetValue(from.entityId.id, out var component)) {
                if (overwrite) {
                    if (this.components.Has(to.entityId.id)) {
                        this.components.Set(to.entityId.id, component, out _);
                    }
                    else {
                        if (this.components.Add(to.entityId.id, component, out _)) {
                            to.AddTransfer(this.typeId);
                        }
                    }
                }
                else {
                    if (this.components.Has(to.entityId.id) == false) {
                        if (this.components.Add(to.entityId.id, component, out _)) {
                            to.AddTransfer(this.typeId);
                        }
                    }
                }

                if (this.components.Remove(from.entityId.id, out _)) {
                    from.RemoveTransfer(this.typeId);
                }
            }
        }


        public override void Dispose() {
            world.ThreadSafetyCheck();
            
            if (this.componentDispose != null) {
                foreach (var componentId in this.components) {
                    this.componentDispose.Invoke(ref this.components.data[componentId]);
                }
            }

            this.components.Clear();
            this.components = null;

            typedStashes.RemoveSwap(this, out _);
            stashes.RemoveSwap(this, out _);

            this.componentDispose = null;
        }
    }

}
