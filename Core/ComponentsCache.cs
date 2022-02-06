#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class ComponentsCache : IDisposable {
        internal static FastList<ComponentsCache> caches = new FastList<ComponentsCache>();

        internal static Action cleanup = () => caches.Clear();

        [SerializeField]
        internal int commonCacheId;
        [SerializeField]
        internal int typedCacheId;
        [SerializeField]
        internal int typeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool RemoveComponent(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract bool Clean(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(Entity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void MigrateComponent(Entity from, Entity to, bool overwrite = true);

        public abstract void Dispose();
    }

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class ComponentsCache<T> : ComponentsCache where T : struct, IComponent {
        internal static FastList<ComponentsCache<T>> typedCaches = new FastList<ComponentsCache<T>>();

        [SerializeField]
        internal IntHashMap<T> components;

        static ComponentsCache() {
            cleanup += () => typedCaches.Clear();
        }

        static void Refill() {
            var id = TypeIdentifier<T>.info.id;
            if (typedCaches == null) {
                typedCaches = new FastList<ComponentsCache<T>>();
            }
            typedCaches.Clear();

            foreach (var cache in caches) {
                if (cache.typeId == id) {
                    typedCaches.Add((ComponentsCache<T>)cache);
                }
            }
        }

        internal ComponentsCache() {
            this.typeId = TypeIdentifier<T>.info.id;

            this.components = new IntHashMap<T>(Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY);

            this.components.Add(-1, default, out _);

            this.typedCacheId = typedCaches.length;
            typedCaches.Add(this);

            this.commonCacheId = caches.length;
            caches.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying AddComponent on null or disposed entity {entity.internalID}");
            }
#endif
            if (this.components.Add(entity.internalID, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId);
                return ref this.components.data[slotIndex];
            }
#if MORPEH_DEBUG
            MDebug.LogError($"You're trying to add on entity {entity.internalID} a component that already exists! Use Get or SetComponent instead!");
#endif
            return ref this.components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(Entity entity, out bool exist) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying AddComponent on null or disposed entity {entity.internalID}");
            }
#endif
            if (this.components.Add(entity.internalID, default, out var slotIndex)) {
                entity.AddTransfer(this.typeId);
                exist = false;
                return ref this.components.data[slotIndex];
            }

            exist = true;
            return ref this.components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddComponent(Entity entity, in T value) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying AddComponent on null or disposed entity {entity.internalID}");
            }
#endif
            if (this.components.Add(entity.internalID, value, out _)) {
                entity.AddTransfer(this.typeId);
                return true;
            }

#if MORPEH_DEBUG
            MDebug.LogError($"You're trying to add on entity {entity.internalID} a component that already exists! Use Get or SetComponent instead!");
#endif
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying GetComponent on null or disposed entity {entity.internalID}");
            }
            
            if (!this.components.Has(entity.internalID)) {
                throw new Exception($"[MORPEH] You're trying to get on entity {entity.internalID} a component that doesn't exists!");
            }
#endif
            return ref this.components.GetValueRefByKey(entity.internalID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T TryGetComponent(Entity entity, out bool exist) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying TryGetComponent on null or disposed entity {entity.internalID}");
            }
#endif
            return ref this.components.TryGetValueRefByKey(entity.internalID, out exist);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent(Entity entity, in T value = default) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying SetComponent on null or disposed entity {entity.internalID}");
            }
#endif

            if (this.components.Set(entity.internalID, value, out _)) {
                entity.AddTransfer(this.typeId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.components.data[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool RemoveComponent(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying RemoveComponent on null or disposed entity {entity.internalID}");
            }
#endif

            if (this.components.Remove(entity.internalID, out _)) {
                entity.RemoveTransfer(this.typeId);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool Clean(Entity entity) => this.components.Remove(entity.internalID, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Has(Entity entity) {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Has on null or disposed entity {entity.internalID}");
            }
#endif
            
            return this.components.Has(entity.internalID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void MigrateComponent(Entity from, Entity to, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying MigrateComponent FROM null or disposed entity {from.internalID}");
            }
            if (to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying MigrateComponent TO null or disposed entity {to.internalID}");
            }
#endif

            if (this.components.TryGetValue(from.internalID, out var component)) {
                if (overwrite) {
                    if (this.components.Has(to.internalID)) {
                        this.components.Set(to.internalID, component, out _);
                    }
                    else {
                        this.components.Add(to.internalID, component, out _);
                    }
                }
                else {
                    if (this.components.Has(to.internalID) == false) {
                        this.components.Add(to.internalID, component, out _);
                    }
                }
                this.components.Remove(from.internalID, out _);
            }
        }

        public override void Dispose() {
            this.components = null;

            typedCaches.RemoveSwap(this, out _);
            caches.RemoveSwap(this, out _);
        }
    }

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class ComponentsCacheDisposable<T> : ComponentsCache<T> where T : struct, IComponent, IDisposable {
        public override bool RemoveComponent(Entity entity) {
            this.components.GetValueRefByKey(entity.internalID).Dispose();
            return base.RemoveComponent(entity);
        }

        public override void Dispose() {
            for (int i = 0, length = this.components.length; i < length; i++) {
                this.components.data[i].Dispose();
            }

            base.Dispose();
        }
    }
}
