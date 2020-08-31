[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests.Runtime")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests.Editor")]

namespace Morpeh {
    using Sirenix.OdinInspector;
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if UNITY_2019_1_OR_NEWER
    using JetBrains.Annotations;
    using UnityEngine;
    using Object = UnityEngine.Object;
#endif
    using Utils;
    using Unity.IL2CPP.CompilerServices;
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;
    using System.Runtime.CompilerServices;

    internal static class Constants {
        internal const int DEFAULT_WORLD_ENTITIES_CAPACITY    = 256;
        internal const int DEFAULT_WORLD_CACHES_CAPACITY      = 12;
        internal const int DEFAULT_ENTITY_COMPONENTS_CAPACITY = 2;
        internal const int DEFAULT_CACHE_COMPONENTS_CAPACITY  = 256;
    }

    public interface IEntity {
        int   ID { get; }
        ref T AddComponent<T>() where T : struct, IComponent;
        ref T AddComponent<T>(out bool exist) where T : struct, IComponent;

        ref T GetComponent<T>() where T : struct, IComponent;
        ref T GetComponent<T>(out bool exist) where T : struct, IComponent;

        void SetComponent<T>(in T value) where T : struct, IComponent;
        bool RemoveComponent<T>() where T : struct, IComponent;
        bool Has<T>() where T : struct, IComponent;
        bool IsDisposed();
    }

    public interface IComponent {
    }

#if UNITY_2019_1_OR_NEWER
    //todo remove
    public interface IMonoComponent<T> : IComponent
        where T : UnityEngine.Component {
        T monoComponent { get; set; }
    }
#endif

    public interface IInitializer : IDisposable {
        World World { get; set; }

        /// <summary>
        ///     Called 1 time on registration in the World
        /// </summary>
        void OnAwake();
    }

    public interface ISystem : IInitializer {
        void OnUpdate(float deltaTime);
    }

    public interface IFixedSystem : ISystem {
    }

    public interface ILateSystem : ISystem {
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    [Serializable]
    internal sealed class Entity : IEntity {
        internal World World => this.backfieldWorld ?? (this.backfieldWorld = World.worlds[this.worldID]);

        [NonSerialized]
        internal World backfieldWorld;

        [SerializeField]
        internal int internalID;
        [SerializeField]
        internal int worldID;

        [SerializeField]
        internal int[] components;
        [SerializeField]
        internal int componentsDoubleCapacity;
        [SerializeField]
        internal int componentsCount;

        [SerializeField]
        private bool isDisposed;

        [SerializeField]
        private int currentArchetype;

        [ShowInInspector]
        public int ID => this.internalID;

        internal Entity(int id, int worldID) {
            this.internalID     = id;
            this.worldID        = worldID;
            this.backfieldWorld = World.worlds[this.worldID];

            this.componentsDoubleCapacity = 0;
            this.componentsCount          = 0;

            this.components = new int[Constants.DEFAULT_ENTITY_COMPONENTS_CAPACITY];
            for (int i = 0, length = this.components.Length; i < length; i++) {
                this.components[i] = -1;
            }

            this.currentArchetype = 0;
            this.World.archetypes[0].Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var world    = this.World;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("[MORPEH] You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
                return ref world.GetCache<T>().Empty();
            }

            if (typeInfo.isMarker) {
                const int componentId = -1;
                for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                    if (this.components[i] == -1) {
                        this.components[i]     = typeInfo.id;
                        this.components[i + 1] = componentId;
                        this.componentsCount++;

                        world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                        return ref world.GetCache<T>().Empty();
                    }
                }

                this.componentsDoubleCapacity += 2;
                if (this.componentsDoubleCapacity >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCapacity << 1);
                    for (int i = this.componentsDoubleCapacity, length = this.componentsDoubleCapacity << 1; i < length; i++) {
                        this.components[i] = -1;
                    }
                }

                this.components[this.componentsDoubleCapacity - 2] = typeInfo.id;
                this.components[this.componentsDoubleCapacity - 1] = componentId;
                this.componentsCount++;
            }
            else {
                var componentId = this.World.GetCache<T>().Add();
                for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                    if (this.components[i] == -1) {
                        this.components[i]     = typeInfo.id;
                        this.components[i + 1] = componentId;
                        this.componentsCount++;

                        world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                        return ref this.World.GetCache<T>().Get(this.components[i + 1]);
                    }
                }

                this.componentsDoubleCapacity += 2;
                if (this.componentsDoubleCapacity >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCapacity << 1);
                    for (int i = this.componentsDoubleCapacity, length = this.componentsDoubleCapacity << 1; i < length; i++) {
                        this.components[i] = -1;
                    }
                }

                this.components[this.componentsDoubleCapacity - 2] = typeInfo.id;
                this.components[this.componentsDoubleCapacity - 1] = componentId;
                this.componentsCount++;

                world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                return ref this.World.GetCache<T>().Get(this.components[this.componentsDoubleCapacity - 1]);
            }

            world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);

            return ref world.GetCache<T>().Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var world    = this.World;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("[MORPEH] You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
                exist = true;
                return ref world.GetCache<T>().Empty();
            }

            exist = false;

            if (typeInfo.isMarker) {
                const int componentId = -1;
                for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                    if (this.components[i] == -1) {
                        this.components[i]     = typeInfo.id;
                        this.components[i + 1] = componentId;
                        this.componentsCount++;

                        world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                        return ref world.GetCache<T>().Empty();
                    }
                }

                this.componentsDoubleCapacity += 2;
                if (this.componentsDoubleCapacity >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCapacity << 1);
                    for (int i = this.componentsDoubleCapacity, length = this.componentsDoubleCapacity << 1; i < length; i++) {
                        this.components[i] = -1;
                    }
                }

                this.components[this.componentsDoubleCapacity - 2] = typeInfo.id;
                this.components[this.componentsDoubleCapacity - 1] = componentId;
                this.componentsCount++;
            }
            else {
                var componentId = world.GetCache<T>().Add();
                for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                    if (this.components[i] == -1) {
                        this.components[i]     = typeInfo.id;
                        this.components[i + 1] = componentId;
                        this.componentsCount++;

                        world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                        return ref world.GetCache<T>().Get(this.components[i + 1]);
                    }
                }

                this.componentsDoubleCapacity += 2;
                if (this.componentsDoubleCapacity >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCapacity << 1);
                    for (int i = this.componentsDoubleCapacity, length = this.componentsDoubleCapacity << 1; i < length; i++) {
                        this.components[i] = -1;
                    }
                }

                this.components[this.componentsDoubleCapacity - 2] = typeInfo.id;
                this.components[this.componentsDoubleCapacity - 1] = componentId;
                this.componentsCount++;

                world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                return ref world.GetCache<T>().Get(this.components[this.componentsDoubleCapacity - 1]);
            }

            return ref world.GetCache<T>().Empty();
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetComponentId(int typeId) {
            var typeInfo = CommonCacheTypeIdentifier.editorTypeAssociation[typeId].typeInfo;
            if (typeInfo.isMarker) {
                return -1;
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    return this.components[i + 1];
                }
            }

            return -1;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetComponentId<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            if (typeInfo.isMarker) {
                return -1;
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    return this.components[i + 1];
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = this.World.GetCache<T>();

            if (typeInfo.isMarker) {
                if (this.Has<T>()) {
                    return ref cache.Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    return ref cache.Get(this.components[i + 1]);
                }
            }

#if UNITY_EDITOR
            Debug.LogError("[MORPEH] You're trying to get a component that doesn't exists!");
#endif
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = this.World.GetCache<T>();

            if (typeInfo.isMarker) {
                if (this.Has<T>()) {
                    exist = true;
                    return ref cache.Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    exist = true;
                    return ref cache.Get(this.components[i + 1]);
                }
            }

            exist = false;
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var world    = this.World;

            if (!typeInfo.isMarker) {
                if (!this.Has(typeInfo.id)) {
                    var componentId = this.World.GetCache<T>().Add();

                    for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                        if (this.components[i] == -1) {
                            this.components[i]     = typeInfo.id;
                            this.components[i + 1] = componentId;
                            this.componentsCount++;

                            world.GetCache<T>().Set(componentId, value);

                            world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                            return;
                        }
                    }

                    this.componentsDoubleCapacity += 2;
                    if (this.componentsDoubleCapacity >= this.components.Length) {
                        Array.Resize(ref this.components, this.componentsDoubleCapacity << 1);
                        for (int i = this.componentsDoubleCapacity, length = this.componentsDoubleCapacity << 1; i < length; i++) {
                            this.components[i] = -1;
                        }
                    }

                    this.components[this.componentsDoubleCapacity - 2] = typeInfo.id;
                    this.components[this.componentsDoubleCapacity - 1] = componentId;
                    this.componentsCount++;

                    world.GetCache<T>().Set(componentId, value);

                    world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                    return;
                }

                for (int i = 0, length = this.components.Length; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        world.GetCache<T>().Set(this.components[i + 1], value);
                    }
                }
            }
            else {
                if (!this.Has(typeInfo.id)) {
                    const int componentId = -1;
                    for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                        if (this.components[i] == -1) {
                            this.components[i]     = typeInfo.id;
                            this.components[i + 1] = componentId;
                            this.componentsCount++;

                            world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                            return;
                        }
                    }

                    this.componentsDoubleCapacity += 2;
                    if (this.componentsDoubleCapacity >= this.components.Length) {
                        Array.Resize(ref this.components, this.componentsDoubleCapacity << 1);
                        for (int i = this.componentsDoubleCapacity, length = this.componentsDoubleCapacity << 1; i < length; i++) {
                            this.components[i] = -1;
                        }
                    }

                    this.components[this.componentsDoubleCapacity - 2] = typeInfo.id;
                    this.components[this.componentsDoubleCapacity - 1] = componentId;
                    this.componentsCount++;

                    world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, true, out this.currentArchetype);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var world    = this.World;

            for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    this.components[i] = -1;
                    if (!typeInfo.isMarker) {
                        world.GetCache<T>().Remove(this.components[i + 1]);
                        this.components[i + 1] = -1;
                    }

                    --this.componentsCount;
                    world.archetypes[this.currentArchetype].Transfer(this.internalID, typeInfo.id, false, out this.currentArchetype);

                    if (this.componentsCount <= 0) {
                        world.RemoveEntity(this);
                    }

                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Has(int typeID) {
            for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                if (this.components[i] == typeID) {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has<T>() where T : struct, IComponent {
            var typeID = CacheTypeIdentifier<T>.info.id;
            for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                if (this.components[i] == typeID) {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDisposed() => this.isDisposed;

        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            var world = this.World;
            world.archetypes[this.currentArchetype].Remove(this.internalID);

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                var typeId      = this.components[i];
                var componentId = this.components[i + 1];
                if (typeId >= 0 && componentId >= 0) {
                    world.GetCache(typeId)?.Remove(componentId);
                }
            }

            this.DisposeFast();
        }

        internal void DisposeFast() {
            this.components               = null;
            this.backfieldWorld           = null;
            this.internalID               = -1;
            this.worldID                  = -1;
            this.componentsDoubleCapacity = -1;
            this.componentsCount          = -1;
            this.currentArchetype         = -1;

            this.isDisposed = true;
        }
    }

    public static class EntityExtensions {
        public static bool IsNullOrDisposed([CanBeNull] this IEntity entity) => entity == null || entity.IsDisposed();
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class SystemsGroup : IDisposable {
        [ShowInInspector]
        private List<ISystem> systems;
        [ShowInInspector]
        private List<ISystem> fixedSystems;
        [ShowInInspector]
        private List<ISystem> lateSystems;

        [ShowInInspector]
        private List<ISystem> disabledSystems;
        [ShowInInspector]
        private List<ISystem> disabledFixedSystems;
        [ShowInInspector]
        private List<ISystem> disabledLateSystems;

        [ShowInInspector]
        private List<IInitializer> newInitializers;
        [ShowInInspector]
        private List<IInitializer> initializers;
        [ShowInInspector]
        private List<IDisposable> disposables;
        private World  world;
        private Action delayedAction;

        private SystemsGroup() {
        }

        internal SystemsGroup(World world) {
            this.world         = world;
            this.delayedAction = null;

            this.systems      = new List<ISystem>();
            this.fixedSystems = new List<ISystem>();
            this.lateSystems  = new List<ISystem>();

            this.disabledSystems      = new List<ISystem>();
            this.disabledFixedSystems = new List<ISystem>();
            this.disabledLateSystems  = new List<ISystem>();

            this.newInitializers = new List<IInitializer>();
            this.initializers    = new List<IInitializer>();
            this.disposables     = new List<IDisposable>();
        }

        public void Dispose() {
            if (this.disposables == null) {
                return;
            }

            void DisposeSystems(List<ISystem> systemsToDispose) {
                foreach (var system in systemsToDispose) {
#if UNITY_EDITOR
                    try {
#endif
                        system.Dispose();
#if UNITY_EDITOR
                    }
                    catch (Exception e) {
                        Debug.LogError($"[MORPEH] Can not dispose system {system.GetType()}");
                        Debug.LogException(e);
                    }
#endif
                }

                systemsToDispose.Clear();
            }

            DisposeSystems(this.systems);
            this.systems = null;

            DisposeSystems(this.fixedSystems);
            this.fixedSystems = null;

            DisposeSystems(this.lateSystems);
            this.lateSystems = null;

            DisposeSystems(this.disabledSystems);
            this.disabledSystems = null;

            DisposeSystems(this.disabledFixedSystems);
            this.disabledFixedSystems = null;

            DisposeSystems(this.disabledLateSystems);
            this.disabledLateSystems = null;

            if (this.newInitializers.Count > 0) {
                foreach (var initializer in this.newInitializers) {
#if UNITY_EDITOR
                    try {
#endif
                        initializer.Dispose();
#if UNITY_EDITOR
                    }
                    catch (Exception e) {
                        Debug.LogError($"[MORPEH] Can not dispose new initializer {initializer.GetType()}");
                        Debug.LogException(e);
                    }
#endif
                }

                this.newInitializers.Clear();
                this.newInitializers = null;

                foreach (var initializer in this.initializers) {
#if UNITY_EDITOR
                    try {
#endif
                        initializer.Dispose();
#if UNITY_EDITOR
                    }
                    catch (Exception e) {
                        Debug.LogError($"[MORPEH] Can not dispose new initializer {initializer.GetType()}");
                        Debug.LogException(e);
                    }
#endif
                }

                this.initializers.Clear();
                this.initializers = null;

                foreach (var disposable in this.disposables) {
#if UNITY_EDITOR
                    try {
#endif
                        disposable.Dispose();
#if UNITY_EDITOR
                    }
                    catch (Exception e) {
                        Debug.LogError($"[MORPEH] Can not dispose system group disposable {disposable.GetType()}");
                        Debug.LogException(e);
                    }
#endif
                }

                this.disposables.Clear();
                this.disposables = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize() {
            if (this.disposables.Count > 0) {
                foreach (var disposable in this.disposables) {
#if UNITY_EDITOR
                    try {
                        disposable.Dispose();
                    }
                    catch (Exception e) {
                        Debug.LogError($"[MORPEH] Can not dispose {disposable.GetType()}");
                        Debug.LogException(e);
                    }
#else
                    disposable.Dispose();
#endif
                }

                this.disposables.Clear();
            }

            this.world.UpdateFilters();
            if (this.newInitializers.Count > 0) {
                foreach (var initializer in this.newInitializers) {
#if UNITY_EDITOR
                    try {
                        initializer.OnAwake();
                    }
                    catch (Exception e) {
                        Debug.LogError($"[MORPEH] Can not initialize {initializer.GetType()}");
                        Debug.LogException(e);
                    }
#else
                    initializer.OnAwake();
#endif

                    this.world.UpdateFilters();
                    this.initializers.Add(initializer);
                }

                this.newInitializers.Clear();
            }
        }

        public void Update(float deltaTime) {
#if UNITY_EDITOR
            this.delayedAction = null;
#endif
            this.Initialize();
            for (int i = 0, length = this.systems.Count; i < length; i++) {
                var system = this.systems[i];
#if UNITY_EDITOR
                try {
                    system.OnUpdate(deltaTime);
                }
                catch (Exception e) {
                    this.SystemThrowException(system, e);
                }
#else
                system.OnUpdate(deltaTime);
#endif
                this.world.UpdateFilters();
            }
#if UNITY_EDITOR
            this.delayedAction?.Invoke();
#endif
        }

        public void FixedUpdate(float deltaTime) {
#if UNITY_EDITOR
            this.delayedAction = null;
#endif
            for (int i = 0, length = this.fixedSystems.Count; i < length; i++) {
                var system = this.fixedSystems[i];
#if UNITY_EDITOR
                try {
                    system.OnUpdate(deltaTime);
                }
                catch (Exception e) {
                    this.SystemThrowException(system, e);
                }
#else
                system.OnUpdate(deltaTime);
#endif
                this.world.UpdateFilters();
            }
#if UNITY_EDITOR
            this.delayedAction?.Invoke();
#endif
        }

        public void LateUpdate(float deltaTime) {
#if UNITY_EDITOR
            this.delayedAction = null;
#endif
            this.world.UpdateFilters();

            for (int i = 0, length = this.lateSystems.Count; i < length; i++) {
                var system = this.lateSystems[i];
#if UNITY_EDITOR
                try {
                    system.OnUpdate(deltaTime);
                }
                catch (Exception e) {
                    this.SystemThrowException(system, e);
                }
#else
                system.OnUpdate(deltaTime);
#endif
                this.world.UpdateFilters();
            }
#if UNITY_EDITOR
            this.delayedAction?.Invoke();
#endif
        }

#if UNITY_EDITOR
        private void SystemThrowException(ISystem system, Exception exception) {
            Debug.LogError($"[MORPEH] Can not update {system.GetType()}. System will be disabled.");
            Debug.LogException(exception);
            this.delayedAction += () => this.DisableSystem(system);
        }
#endif

        public void AddInitializer<T>(T initializer) where T : class, IInitializer {
            initializer.World = this.world;

            this.newInitializers.Add(initializer);
        }

        public void RemoveInitializer<T>(T initializer) where T : class, IInitializer
            => this.newInitializers.Remove(initializer);

        public bool AddSystem<T>(T system, bool enabled = true) where T : class, ISystem {
            var collection         = this.systems;
            var disabledCollection = this.disabledSystems;
            if (system is IFixedSystem) {
                collection         = this.fixedSystems;
                disabledCollection = this.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = this.lateSystems;
                disabledCollection = this.disabledLateSystems;
            }

            if (enabled && !collection.Contains(system)) {
                collection.Add(system);
                this.AddInitializer(system);
                return true;
            }

            if (!enabled && !disabledCollection.Contains(system)) {
                disabledCollection.Add(system);
                this.AddInitializer(system);
                return true;
            }

            return false;
        }

        public bool EnableSystem<T>(T system) where T : class, ISystem {
            var collection         = this.systems;
            var disabledCollection = this.disabledSystems;
            if (system is IFixedSystem) {
                collection         = this.fixedSystems;
                disabledCollection = this.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = this.lateSystems;
                disabledCollection = this.disabledLateSystems;
            }

            if (disabledCollection.Contains(system)) {
                collection.Add(system);
                disabledCollection.Remove(system);
                return true;
            }

            return false;
        }

        public bool DisableSystem<T>(T system) where T : class, ISystem {
            var collection         = this.systems;
            var disabledCollection = this.disabledSystems;
            if (system is IFixedSystem) {
                collection         = this.fixedSystems;
                disabledCollection = this.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = this.lateSystems;
                disabledCollection = this.disabledLateSystems;
            }

            if (collection.Contains(system)) {
                disabledCollection.Add(system);
                collection.Remove(system);
                return true;
            }

            return false;
        }

        public bool RemoveSystem<T>(T system) where T : class, ISystem {
            var collection         = this.systems;
            var disabledCollection = this.disabledSystems;
            if (system is IFixedSystem) {
                collection         = this.fixedSystems;
                disabledCollection = this.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = this.lateSystems;
                disabledCollection = this.disabledLateSystems;
            }

            if (collection.Contains(system)) {
                var deleted = collection.Remove(system);
                if (deleted) {
                    this.disposables.Add(system);
                    this.RemoveInitializer(system);
                    return true;
                }
            }
            else if (disabledCollection.Contains(system)) {
                var deleted = disabledCollection.Remove(system);
                if (deleted) {
                    this.disposables.Add(system);
                    this.RemoveInitializer(system);
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed partial class World : IDisposable {
        [CanBeNull]
        public static World Default => worlds[0];
        [NotNull]
        internal static List<World> worlds = new List<World> {null};

        [NonSerialized]
        public Filter Filter;
        [SerializeField]
        public bool UpdateByUnity;

        [NonSerialized]
        internal List<Filter> filters;

        [NonSerialized]
        internal List<Filter> dirtyFilters;

        [ShowInInspector]
        [NonSerialized]
        internal SortedList<int, SystemsGroup> systemsGroups;

        [ShowInInspector]
        [NonSerialized]
        internal SortedList<int, SystemsGroup> newSystemsGroups;

        [SerializeField]
        internal Entity[] entities;
        [SerializeField]
        internal int entitiesCount;
        [SerializeField]
        internal int entitiesLength;
        [SerializeField]
        internal int entitiesCapacity;

        [SerializeField]
        private List<int> freeEntityIDs;
        [SerializeField]
        private List<int> nextFreeEntityIDs;

        [SerializeField]
        internal CacheComponents[] caches;

        [SerializeField]
        internal List<Archetype> archetypes;
        [SerializeField]
        internal Dictionary<int, List<int>> archetypesByLength;
        [SerializeField]
        internal List<int> changedArchetypes;
        [SerializeField]
        internal List<int> newArchetypes;
        [NonSerialized]
        private List<int> archetypeCache;

        [SerializeField]
        internal int id;

        public static World Create() => new World().Initialize();

        private World() {
            this.Ctor();
        }

        internal void Ctor() {
            this.systemsGroups    = new SortedList<int, SystemsGroup>();
            this.newSystemsGroups = new SortedList<int, SystemsGroup>();

            this.Filter         = new Filter(this);
            this.filters        = new List<Filter>();
            this.dirtyFilters   = new List<Filter>();
            this.archetypeCache = new List<int>();

            if (this.archetypes != null) {
                foreach (var archetype in this.archetypes) {
                    archetype.Ctor();
                }
            }

            this.InitializeGlobals();
        }

        partial void InitializeGlobals();

        private World Initialize() {
            worlds.Add(this);
            this.id                = worlds.Count - 1;
            this.freeEntityIDs     = new List<int>();
            this.nextFreeEntityIDs = new List<int>();
            this.caches            = new CacheComponents[Constants.DEFAULT_WORLD_CACHES_CAPACITY];

            this.entitiesLength   = 0;
            this.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            this.entities         = new Entity[this.entitiesCapacity];

            this.archetypes = new List<Archetype> {new Archetype(0, new int[0], this.id)};
            this.archetypesByLength = new Dictionary<int, List<int>> {
                [0] = new List<int> {0}
            };
            this.changedArchetypes = new List<int>();
            this.newArchetypes     = new List<int>();


            return this;
        }

        public void Dispose() {
            foreach (var systemsGroup in this.systemsGroups.Values) {
#if UNITY_EDITOR
                try {
#endif
                    systemsGroup.Dispose();
#if UNITY_EDITOR
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose system group {systemsGroup.GetType()}");
                    Debug.LogException(e);
                }
#endif
            }

            this.systemsGroups = null;

            foreach (var entity in this.entities) {
#if UNITY_EDITOR
                try {
#endif
                    entity?.DisposeFast();
#if UNITY_EDITOR
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose entity with ID {entity?.ID}");
                    Debug.LogException(e);
                }
#endif
            }

            this.entities         = null;
            this.entitiesLength   = -1;
            this.entitiesCapacity = -1;

            this.freeEntityIDs.Clear();
            this.freeEntityIDs = null;
            this.nextFreeEntityIDs.Clear();
            this.nextFreeEntityIDs = null;
#if UNITY_EDITOR
            try {
#endif
                this.Filter.Dispose();
#if UNITY_EDITOR
            }
            catch (Exception e) {
                Debug.LogError("[MORPEH] Can not dispose root filter");
                Debug.LogException(e);
            }
#endif
            this.Filter = null;

            this.filters.Clear();
            this.filters = null;

            this.dirtyFilters.Clear();
            this.dirtyFilters = null;

            foreach (var cache in this.caches) {
#if UNITY_EDITOR
                try {
#endif
                    cache?.Dispose();
#if UNITY_EDITOR
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose cache {cache?.GetType()}");
                    Debug.LogException(e);
                }
#endif
            }

            this.caches = null;

            foreach (var archetype in this.archetypes) {
                archetype.Dispose();
            }

            this.archetypes.Clear();
            this.archetypes = null;

            foreach (var pair in this.archetypesByLength) {
                pair.Value.Clear();
            }

            this.archetypesByLength.Clear();
            this.archetypesByLength = null;

            this.newArchetypes.Clear();
            this.newArchetypes = null;

            this.changedArchetypes.Clear();
            this.changedArchetypes = null;

            worlds.Remove(this);
        }
#if UNITY_2019_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void InitializationDefaultWorld() {
            worlds.Clear();
            var defaultWorld = Create();
            defaultWorld.UpdateByUnity = true;
#if UNITY_2019_1_OR_NEWER
            var go = new GameObject {
                name      = "MORPEH_UNITY_RUNTIME_HELPER",
                hideFlags = HideFlags.DontSaveInEditor
            };
            go.AddComponent<UnityRuntimeHelper>();
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);
#endif
        }


        //TODO refactor allocations and fast sort(maybe without it?)
        internal Archetype GetArchetype(int[] typeIds, int newTypeId, bool added, out int archetypeId) {
            Archetype archetype = null;
            archetypeId = -1;

            this.archetypeCache.Clear();
            for (int i = 0, length = typeIds.Length; i < length; i++) {
                var typeId = typeIds[i];
                if (typeId >= 0) {
                    this.archetypeCache.Add(typeIds[i]);
                }
            }

            if (added) {
                this.archetypeCache.Add(newTypeId);
            }
            else {
                this.archetypeCache.Remove(newTypeId);
            }

            this.archetypeCache.Sort();
            var typesLength = this.archetypeCache.Count;

            if (this.archetypesByLength.TryGetValue(typesLength, out var archsl)) {
                for (var index = 0; index < archsl.Count; index++) {
                    archetypeId = archsl[index];
                    archetype   = this.archetypes[archetypeId];
                    var check = true;
                    for (int i = 0, length = typesLength; i < length; i++) {
                        if (archetype.typeIds[i] != this.archetypeCache[i]) {
                            check = false;
                            break;
                        }
                    }

                    if (check) {
                        return archetype;
                    }
                }
            }

            archetypeId = this.archetypes.Count;
            var newArchetype = new Archetype(archetypeId, this.archetypeCache.ToArray(), this.id);
            this.archetypes.Add(newArchetype);
            if (this.archetypesByLength.TryGetValue(typesLength, out archsl)) {
                archsl.Add(archetypeId);
            }
            else {
                this.archetypesByLength.Add(typesLength, new List<int> {archetypeId});
            }

            this.newArchetypes.Add(archetypeId);

            archetype = newArchetype;

            return archetype;
        }

        internal CacheComponents GetCache(int typeId) => this.caches[typeId];

        internal CacheComponents<T> GetCache<T>() where T : struct, IComponent {
            var info          = CacheTypeIdentifier<T>.info;
            var currentLength = this.caches.Length;
            if (info.id >= currentLength) {
                while (currentLength <= info.id) {
                    currentLength <<= 1;
                }

                Array.Resize(ref this.caches, currentLength);
            }

            var cache = (CacheComponents<T>) this.caches[info.id];
            if (cache == null) {
                if (info.isDisposable) {
                    var constructedType          = typeof(CacheDisposableComponents<>).MakeGenericType(typeof(T));
                    this.caches[info.id] = cache = (CacheComponents<T>) Activator.CreateInstance(constructedType);
                }
                else {
                    this.caches[info.id] = cache = new CacheComponents<T>();
                }
            }

            return cache;
        }

        public static void GlobalUpdate(float deltaTime) {
            foreach (var world in worlds) {
                if (world.UpdateByUnity) {
                    world.Update(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime) {
            for (var i = 0; i < this.newSystemsGroups.Count; i++) {
                var key          = this.newSystemsGroups.Keys[i];
                var systemsGroup = this.newSystemsGroups.Values[i];

                systemsGroup.Initialize();
                this.systemsGroups.Add(key, systemsGroup);
            }

            this.newSystemsGroups.Clear();

            for (var i = 0; i < this.systemsGroups.Count; i++) {
                var systemsGroup = this.systemsGroups.Values[i];
                systemsGroup.Update(deltaTime);
            }
        }

        public static void GlobalFixedUpdate(float deltaTime) {
            foreach (var world in worlds) {
                if (world.UpdateByUnity) {
                    world.FixedUpdate(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FixedUpdate(float deltaTime) {
            for (var i = 0; i < this.systemsGroups.Count; i++) {
                var systemsGroup = this.systemsGroups.Values[i];
                systemsGroup.FixedUpdate(deltaTime);
            }
        }

        public static void GlobalLateUpdate(float deltaTime) {
            foreach (var world in worlds) {
                if (world.UpdateByUnity) {
                    world.LateUpdate(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LateUpdate(float deltaTime) {
            for (var i = 0; i < this.systemsGroups.Count; i++) {
                var systemsGroup = this.systemsGroups.Values[i];
                systemsGroup.LateUpdate(deltaTime);
            }
        }

        public SystemsGroup CreateSystemsGroup() => new SystemsGroup(this);

        public void AddSystemsGroup(int order, SystemsGroup systemsGroup) {
            this.newSystemsGroups.Add(order, systemsGroup);
        }

        public void RemoveSystemsGroup(SystemsGroup systemsGroup) {
            systemsGroup.Dispose();
            if (this.systemsGroups.ContainsValue(systemsGroup)) {
                this.systemsGroups.RemoveAt(this.systemsGroups.IndexOfValue(systemsGroup));
            }
            else if (this.newSystemsGroups.ContainsValue(systemsGroup)) {
                this.newSystemsGroups.RemoveAt(this.newSystemsGroups.IndexOfValue(systemsGroup));
            }
        }

        public IEntity CreateEntity() => this.CreateEntityInternal();

        internal Entity CreateEntityInternal() {
            var id = -1;
            if (this.freeEntityIDs.Count > 0) {
                id = this.freeEntityIDs[0];
                this.freeEntityIDs.RemoveAtFast(0);
            }
            else {
                id = this.entitiesLength++;
            }

            if (this.entitiesLength >= this.entitiesCapacity) {
                var newCapacity = this.entitiesCapacity << 1;
                Array.Resize(ref this.entities, newCapacity);
                this.entitiesCapacity = newCapacity;
            }

            this.entities[id] = new Entity(id, worlds.IndexOf(this));
            ++this.entitiesCount;

            return this.entities[id];
        }

        public IEntity CreateEntity(out int id) => this.CreateEntityInternal(out id);

        internal Entity CreateEntityInternal(out int id) {
            if (this.freeEntityIDs.Count > 0) {
                id = this.freeEntityIDs[0];
                this.freeEntityIDs.RemoveAtFast(0);
            }
            else {
                id = this.entitiesLength++;
            }

            if (this.entitiesLength >= this.entitiesCapacity) {
                var newCapacity = this.entitiesCapacity << 1;
                Array.Resize(ref this.entities, newCapacity);
                this.entitiesCapacity = newCapacity;
            }


            this.entities[id] = new Entity(id, worlds.IndexOf(this));
            ++this.entitiesCount;
            return this.entities[id];
        }

        [CanBeNull]
        public IEntity GetEntity(in int id) => this.entities[id];

        [CanBeNull]
        internal Entity GetEntityInternal(in int id) => this.entities[id];

        public void RemoveEntity(IEntity entity) {
            if (entity is Entity ent) {
                var id = ent.ID;
                if (this.entities[id] == ent) {
                    this.nextFreeEntityIDs.Add(id);
                    this.entities[id] = null;
                    --this.entitiesCount;
                    ent.Dispose();
                }
            }
        }

        public void UpdateFilters() {
            if (this.newArchetypes.Count > 0) {
                foreach (var filter in this.filters) {
                    filter.FindArchetypes(this.newArchetypes);
                }

                this.newArchetypes.Clear();
            }

            if (this.changedArchetypes.Count > 0) {
                foreach (var archId in this.changedArchetypes) {
                    this.archetypes[archId].Swap();
                }

                this.changedArchetypes.Clear();
            }

            if (this.dirtyFilters.Count > 0) {
                foreach (var filter in this.dirtyFilters) {
                    filter.InternalUpdate();
                }

                this.dirtyFilters.Clear();
            }

            if (this.nextFreeEntityIDs.Count > 0) {
                this.freeEntityIDs.AddRange(this.nextFreeEntityIDs);
                this.nextFreeEntityIDs.Clear();
            }
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class Archetype : IDisposable {
        [SerializeField]
        public int[] typeIds;
        [SerializeField]
        public bool isDirty;
        [SerializeField]
        public int[] currentEntities;
        [SerializeField]
        public int length;
        [NonSerialized]
        public List<Filter> filters;
        [SerializeField]
        internal List<int> entities;
        [SerializeField]
        internal Dictionary<int, int> removeTransfer;
        [SerializeField]
        internal Dictionary<int, int> addTransfer;
        [SerializeField]
        internal int worldId;
        [SerializeField]
        internal int id;
        [NonSerialized]
        internal World backfieldWorld;
        internal World world {
            get {
                if (this.backfieldWorld == null) {
                    this.backfieldWorld = World.worlds[this.worldId];
                }

                return this.backfieldWorld;
            }
        }

        internal Archetype(int id, int[] typeIds, int worldId) {
            this.id              = id;
            this.typeIds         = typeIds;
            this.entities        = new List<int>(16);
            this.addTransfer     = new Dictionary<int, int>();
            this.removeTransfer  = new Dictionary<int, int>();
            this.currentEntities = new int[16];
            this.isDirty         = false;
            this.worldId         = worldId;

            this.Ctor();
        }

        internal void Ctor() {
            this.backfieldWorld = World.worlds[this.worldId];
            this.filters        = new List<Filter>();
        }

        public void Add(int entityId) {
            this.entities.Add(entityId);
            this.isDirty = true;
            this.OnChange();
        }

        public void Remove(int entityId) {
            if (this.entities.Remove(entityId)) {
                this.isDirty = true;
                this.OnChange();
            }
        }

        public void AddFilter(Filter filter) {
            this.filters.Add(filter);
            this.OnChange();
        }

        public void RemoveFilter(Filter filter) {
            if (this.filters.Remove(filter)) {
                this.OnChange();
            }
        }

        private void OnChange() {
            this.world.changedArchetypes.Add(this.id);
            foreach (var filter in this.filters) {
                filter.MakeDirty();
            }
        }

        public void Swap() {
            if (!this.isDirty) {
                return;
            }

            var cap = this.entities.Capacity;
            if (cap > this.currentEntities.Length) {
                Array.Resize(ref this.currentEntities, cap);
            }

            this.entities.CopyTo(this.currentEntities);
            this.length  = this.entities.Count;
            this.isDirty = false;
        }

        public void Transfer(int entityId, int typeId, bool added, out int archetypeId) {
            this.Remove(entityId);
            if (added) {
                if (this.addTransfer.TryGetValue(typeId, out archetypeId)) {
                    this.world.archetypes[archetypeId].Add(entityId);
                }
                else {
                    var arch = this.world.GetArchetype(this.typeIds, typeId, added, out archetypeId);
                    arch.Add(entityId);
                    this.addTransfer.Add(typeId, archetypeId);
                }
            }
            else {
                if (this.removeTransfer.TryGetValue(typeId, out archetypeId)) {
                    this.world.archetypes[archetypeId].Add(entityId);
                }
                else {
                    var arch = this.world.GetArchetype(this.typeIds, typeId, added, out archetypeId);
                    arch.Add(entityId);
                    this.removeTransfer.Add(typeId, archetypeId);
                }
            }
        }

        public void Dispose() {
            this.isDirty = false;

            this.typeIds        = null;
            this.backfieldWorld = null;

            this.entities.Clear();
            this.entities        = null;
            this.currentEntities = null;

            this.addTransfer.Clear();
            this.addTransfer = null;

            this.removeTransfer.Clear();
            this.removeTransfer = null;
        }
    }

    //TODO Separate RootFilter and ChildFilter
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class Filter : IEnumerable<IEntity>, IDisposable {
        private enum FilterMode {
            None    = 0,
            Include = 1,
            Exclude = 2
        }

        public           int   Length;
        private readonly World world;

        private int[] entitiesCacheForBags;
        private int   entitiesCacheForBagsCapacity;

        // 0 - typeIndex
        // 1 - componentsBagCacheIndex
        // 2 - isDirty
        private int[] componentsBags;
        private int   componentsBagsTripleCount;

        private List<Filter> childs;

        private List<int> includedTypeIds;
        private List<int> excludedTypeIds;

        private List<Archetype> archetypes;

        private int        typeID;
        private FilterMode filterMode;

        private bool isDirty;
        private bool isDirtyCache;

        //root filter ctor
        //don't allocate any trash
        internal Filter(World world) {
            this.world = world;

            this.childs = new List<Filter>();

            this.typeID     = -1;
            this.filterMode = FilterMode.Include;

            this.entitiesCacheForBags         = new int[0];
            this.entitiesCacheForBagsCapacity = 0;
        }

        //full child filter
        private Filter(World world, int typeID, List<int> includedTypeIds, List<int> excludedTypeIds, FilterMode mode) {
            this.world = world;

            this.childs     = new List<Filter>();
            this.archetypes = new List<Archetype>();

            this.typeID          = typeID;
            this.includedTypeIds = includedTypeIds;
            this.excludedTypeIds = excludedTypeIds;

            this.filterMode = mode;

            this.componentsBags       = new int[0];
            this.entitiesCacheForBags = new int[0];

            this.componentsBagsTripleCount    = 0;
            this.entitiesCacheForBagsCapacity = 0;
            this.isDirtyCache                 = true;

            this.world.filters.Add(this);

            this.FindArchetypes();

            this.UpdateCache();
        }

        public void Dispose() {
            foreach (var child in this.childs) {
                child.Dispose();
            }

            this.childs.Clear();
            this.childs = null;

            if (this.archetypes != null) {
                foreach (var archetype in this.archetypes) {
                    archetype.RemoveFilter(this);
                }

                this.archetypes.Clear();
                this.archetypes = null;
            }

            this.includedTypeIds?.Clear();
            this.includedTypeIds = null;
            this.excludedTypeIds?.Clear();
            this.excludedTypeIds = null;

            this.Length = -1;

            this.entitiesCacheForBags         = null;
            this.entitiesCacheForBagsCapacity = -1;

            this.componentsBags            = null;
            this.componentsBagsTripleCount = -1;

            this.typeID     = -1;
            this.filterMode = FilterMode.None;
        }

        [Obsolete("Use World.UpdateFilters()")]
        public void Update() {
            this.world.UpdateFilters();
        }

        internal void InternalUpdate() {
            this.UpdateLength();

            for (int i = 0, length = this.componentsBagsTripleCount; i < length; i += 3) {
                this.componentsBags[i + 2] = 1;
            }

            this.isDirty      = false;
            this.isDirtyCache = true;
        }

        public void MakeDirty() {
            if (this.isDirty == false) {
                this.world.dirtyFilters.Add(this);
                this.isDirty = true;
            }
        }

        private void UpdateLength() {
            this.Length = 0;
            for (int i = 0, length = this.archetypes.Count; i < length; i++) {
                this.Length += this.archetypes[i].length;
            }
        }

        private void UpdateCache() {
            this.UpdateLength();

            if (this.entitiesCacheForBagsCapacity < this.Length) {
                Array.Resize(ref this.entitiesCacheForBags, this.Length);
                this.entitiesCacheForBagsCapacity = this.Length;
            }

            var j = 0;
            foreach (var arch in this.archetypes) {
                Array.Copy(arch.currentEntities, 0, this.entitiesCacheForBags, j, arch.length);
                j += arch.length;
            }


            this.isDirtyCache = false;
        }

        internal void FindArchetypes(List<int> newArchetypes) {
            var minLength = this.includedTypeIds.Count;
            foreach (var archId in newArchetypes) {
                var arch = this.world.archetypes[archId];
                this.CheckArchetype(arch, minLength);
            }
        }

        internal void FindArchetypes() {
            var minLength = this.includedTypeIds.Count;
            foreach (var arch in this.world.archetypes) {
                this.CheckArchetype(arch, minLength);
            }
        }

        private void CheckArchetype(Archetype archetype, int minLength) {
            var typeIdsLength = archetype.typeIds.Length;
            if (typeIdsLength >= minLength) {
                var check = true;
                for (int i = 0, length = minLength; i < length; i++) {
                    var includedTypeId = this.includedTypeIds[i];
                    var foundInclude   = false;
                    for (int j = 0, lengthj = typeIdsLength; j < lengthj; j++) {
                        var typeId = archetype.typeIds[j];
                        if (typeId > includedTypeId) {
                            check = false;
                            goto BREAK;
                        }

                        if (typeId == includedTypeId) {
                            foundInclude = true;
                            break;
                        }
                    }

                    if (foundInclude == false) {
                        check = false;
                        goto BREAK;
                    }
                }

                for (int i = 0, length = this.excludedTypeIds.Count; i < length; i++) {
                    var excludedTypeId = this.excludedTypeIds[i];
                    for (int j = 0, lengthj = typeIdsLength; j < lengthj; j++) {
                        var typeId = archetype.typeIds[j];
                        if (typeId > excludedTypeId) {
                            break;
                        }

                        if (typeId == excludedTypeId) {
                            check = false;
                            goto BREAK;
                        }
                    }
                }

                BREAK:
                if (check && !this.archetypes.Contains(archetype)) {
                    this.archetypes.Add(archetype);
                    archetype.AddFilter(this);
                }
            }
        }

        public ref ComponentsBag<T> Select<T>() where T : struct, IComponent {
            if (this.isDirtyCache) {
                this.UpdateCache();
            }

            var typeInfo = CacheTypeIdentifier<T>.info;
            if (typeInfo.isMarker) {
#if UNITY_EDITOR
                Debug.LogError($"You Select<{typeof(T)}> marker component from filter! This makes no sense.");
#endif
                return ref ComponentsBag<T>.Empty;
            }


            for (int i = 0, length = this.componentsBagsTripleCount; i < length; i += 3) {
                if (this.componentsBags[i] == typeInfo.id) {
                    ref var index        = ref this.componentsBags[i + 1];
                    ref var componentBag = ref ComponentsBag<T>.Get(index);

                    if (this.componentsBags[i + 2] > 0) {
                        componentBag.Update(this.entitiesCacheForBags, this.Length);
                        this.componentsBags[i + 2] = 0;
                    }

                    return ref componentBag;
                }
            }

            var newIndexBag = ComponentsBag<T>.Create(this.world);
            this.componentsBagsTripleCount += 3;
            if (this.componentsBagsTripleCount >= this.componentsBags.Length) {
                Array.Resize(ref this.componentsBags, this.componentsBagsTripleCount << 1);
            }

            this.componentsBags[this.componentsBagsTripleCount - 3] = typeInfo.id;
            this.componentsBags[this.componentsBagsTripleCount - 2] = newIndexBag;
            this.componentsBags[this.componentsBagsTripleCount - 1] = 0;

            ref var indexBag         = ref this.componentsBags[this.componentsBagsTripleCount - 2];
            ref var newComponentsBag = ref ComponentsBag<T>.Get(indexBag);
            newComponentsBag.Update(this.entitiesCacheForBags, this.Length);
            return ref newComponentsBag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntity GetEntity(in int id) {
            if (this.isDirtyCache) {
                this.UpdateCache();
            }

            return this.world.entities[this.entitiesCacheForBags[id]];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntity First() {
            if (this.isDirtyCache) {
                this.UpdateCache();
            }

            return this.world.entities[this.entitiesCacheForBags[0]];
        }

        public Filter With<T>() where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Include);

        public Filter Without<T>() where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Exclude);

        private Filter CreateFilter<T>(FilterMode mode) where T : struct, IComponent {
            for (int i = 0, length = this.childs.Count; i < length; i++) {
                var child = this.childs[i];
                if (child.filterMode == mode && child.typeID == CacheTypeIdentifier<T>.info.id) {
                    return child;
                }
            }

            var newTypeId = CacheTypeIdentifier<T>.info.id;

            List<int> includedTypeIds;
            List<int> excludedTypeIds;
            if (this.typeID == -1) {
                includedTypeIds = new List<int>();
                excludedTypeIds = new List<int>();
            }
            else {
                includedTypeIds = new List<int>(this.includedTypeIds);
                excludedTypeIds = new List<int>(this.excludedTypeIds);
            }

            if (mode == FilterMode.Include) {
                includedTypeIds.Add(newTypeId);
            }
            else if (mode == FilterMode.Exclude) {
                excludedTypeIds.Add(newTypeId);
            }

            var newFilter = new Filter(this.world, newTypeId, includedTypeIds, excludedTypeIds, mode);
            this.childs.Add(newFilter);

            return newFilter;
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public struct ComponentsBag<T> where T : struct, IComponent {
            internal static ComponentsBag<T> Empty = new ComponentsBag<T>();

            private static ComponentsBag<T>[] cache;

            private static          int  cacheLength;
            private static          int  cacheCapacity;
            private static readonly bool isMarker;

            private T[]   sharedComponents;
            private int[] ids;
            private World world;

            static ComponentsBag() {
                isMarker = CacheTypeIdentifier<T>.info.isMarker;
                if (isMarker) {
                    return;
                }

                cacheLength   = 0;
                cacheCapacity = 16;
                cache         = new ComponentsBag<T>[cacheCapacity];
            }

            internal void Update(int[] entities, in int len) {
                Array.Resize(ref this.ids, len);
                for (var i = 0; i < len; i++) {
                    this.ids[i] = this.world.entities[entities[i]].GetComponentId<T>();
                }

                this.sharedComponents = this.world.GetCache<T>().components;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T GetComponent(in int index) => ref this.sharedComponents[this.ids[index]];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetComponent(in int index, in T value) => this.sharedComponents[this.ids[index]] = value;

            internal static ref ComponentsBag<T> Get(in int index) => ref cache[index];

            internal static int Create(World world) {
                var bag = new ComponentsBag<T> {
                    world            = world,
                    ids              = new int[1],
                    sharedComponents = world.GetCache<T>().components
                };

                if (cacheCapacity <= cacheLength) {
                    var newCapacity = cacheCapacity * 2;
                    Array.Resize(ref cache, newCapacity);
                    cacheCapacity = newCapacity;
                }

                var index = cacheLength;
                cache[index] = bag;
                cacheLength++;
                return index;
            }
        }

        public EntityEnumerator GetEnumerator() => new EntityEnumerator(this.world, this.archetypes);

        IEnumerator<IEntity> IEnumerable<IEntity>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public struct EntityEnumerator : IEnumerator<IEntity> {
            private World           world;
            private List<Archetype> archetypes;
            private Entity          current;

            private int id;
            private int idList;

            internal EntityEnumerator(World world, List<Archetype> archetypes) {
                this.world      = world;
                this.archetypes = archetypes;
                this.current    = null;

                this.id     = 0;
                this.idList = 0;
            }

            public bool MoveNext() {
                if (this.idList < this.archetypes.Count) {
                    var arch = this.archetypes[this.idList];
                    while (this.id >= arch.length) {
                        this.id = 0;
                        this.idList++;
                        if (this.idList < this.archetypes.Count) {
                            arch = this.archetypes[this.idList];
                        }
                        else {
                            return false;
                        }
                    }

                    if (this.id < arch.length) {
                        var entId = arch.currentEntities[this.id];
                        this.current = this.world.entities[entId];
                        this.id++;

                        return true;
                    }
                }

                return false;
            }

            public void Reset() {
                this.current = null;
                this.id      = 0;
                this.idList  = 0;
            }

            public IEntity Current => this.current;

            object IEnumerator.Current => this.current;

            public void Dispose() {
                this.id     = -1;
                this.idList = -1;

                this.current = null;
                this.world   = null;
            }
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal static class CommonCacheTypeIdentifier { 
        private static int counter;

        internal static int GetID() => counter++;
#if UNITY_EDITOR
        internal static Dictionary<int, DebugInfo> editorTypeAssociation = new Dictionary<int, DebugInfo>();

        internal static int GetID<T>() where T : struct, IComponent {
            var id = counter++;
            var info = new DebugInfo {
                type     = typeof(T),
                getBoxed = (world, componentId) => world.GetCache<T>().components[componentId],
                setBoxed = (world, componentId, value) => world.GetCache<T>().components[componentId] = (T) value,
                typeInfo = CacheTypeIdentifier<T>.info
            };
            editorTypeAssociation.Add(id, info);
            return id;
        }

        internal struct DebugInfo {
            public Type                       type;
            public Func<World, int, object>   getBoxed;
            public Action<World, int, object> setBoxed;
            public TypeInfo                   typeInfo;
        }
#endif

        [Serializable]
        internal class TypeInfo {
            [SerializeField]
            internal int id;
            [SerializeField]
            internal bool isMarker;
            [SerializeField]
            internal bool isDisposable;

            public TypeInfo(bool isMarker, bool isDisposable) {
                this.isMarker     = isMarker;
                this.isDisposable = isDisposable;
            }

            public void SetID(int id) {
                this.id = id;
            }
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal static class CacheTypeIdentifier<T> where T : struct, IComponent {
        internal static CommonCacheTypeIdentifier.TypeInfo info;

        static CacheTypeIdentifier() {
            info = new CommonCacheTypeIdentifier.TypeInfo(UnsafeUtility.SizeOf<T>() == 1, typeof(IDisposable).IsAssignableFrom(typeof(T)));
#if UNITY_EDITOR
            var id = CommonCacheTypeIdentifier.GetID<T>();
#else
            var id = CommonCacheTypeIdentifier.GetID();
#endif
            info.SetID(id);
        }
    }

    internal static class UnsafeUtility {
        public static int SizeOf<T>() where T : struct {
#if UNITY_2019_1_OR_NEWER
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<T>();
#else
            return System.Runtime.InteropServices.Marshal.SizeOf(default(T));
#endif
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal abstract class CacheComponents : IDisposable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Remove(in int id);

        public abstract void Dispose();
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal class CacheComponents<T> : CacheComponents where T : struct, IComponent {
        [SerializeField]
        internal T[] components;
        [SerializeField]
        protected int capacity;
        [SerializeField]
        protected int length;
        [SerializeField]
        protected List<int> freeIndexes;

        public CacheComponents() {
            this.capacity = Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY;
            this.length   = 1;

            this.components  = new T[this.capacity];
            this.freeIndexes = new List<int>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add() {
            var id = 0;

            if (this.freeIndexes.Count > 0) {
                id = this.freeIndexes[0];
                this.freeIndexes.RemoveAtFast(0);
                return id;
            }

            if (this.capacity <= this.length) {
                var newCapacity = this.capacity * 2;
                Array.Resize(ref this.components, newCapacity);
                this.capacity = newCapacity;
            }

            id = this.length;
            this.length++;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(in int id) => ref this.components[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(in int id, in T value) => this.components[id] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Empty() => ref this.components[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Remove(in int id) {
            this.components[id] = default;

            if (this.length >= id) {
                this.freeIndexes.Add(id);
                return true;
            }

            return false;
        }

        public override void Dispose() {
            this.components = null;
            this.capacity   = -1;
            this.length     = -1;
            this.freeIndexes.Clear();
            this.freeIndexes = null;
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal sealed class CacheDisposableComponents<T> : CacheComponents<T> where T : struct, IComponent, IDisposable {
        public override bool Remove(in int id) {
            this.components[id].Dispose();
            return base.Remove(in id);
        }

        public override void Dispose() {
            for (int i = 0; i < this.length; i++) {
                this.components[i].Dispose();
            }

            base.Dispose();
        }
    }

    namespace Utils {
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public static class ListExtensions {
            //remove with swap last and removed
            public static void RemoveAtFast<T>(this IList<T> list, int index) {
                var count = list.Count;
                list[index] = list[count - 1];
                list.RemoveAt(count - 1);
            }

            //remove with swap last and removed
            public static void RemoveFast<T>(this IList<T> list, T item) {
                var count = list.Count;
                var index = list.IndexOf(item);
                list[index] = list[count - 1];
                list.RemoveAt(count - 1);
            }
        }
    }
}

namespace Unity.IL2CPP.CompilerServices {
    using System;

    /// <summary>
    ///     The code generation options available for IL to C++ conversion.
    ///     Enable or disabled these with caution.
    /// </summary>
    public enum Option {
        /// <summary>
        ///     Enable or disable code generation for null checks.
        ///     Global null check support is enabled by default when il2cpp.exe
        ///     is launched from the Unity editor.
        ///     Disabling this will prevent NullReferenceException exceptions from
        ///     being thrown in generated code. In *most* cases, code that dereferences
        ///     a null pointer will crash then. Sometimes the point where the crash
        ///     happens is later than the location where the null reference check would
        ///     have been emitted though.
        /// </summary>
        NullChecks = 1,

        /// <summary>
        ///     Enable or disable code generation for array bounds checks.
        ///     Global array bounds check support is enabled by default when il2cpp.exe
        ///     is launched from the Unity editor.
        ///     Disabling this will prevent IndexOutOfRangeException exceptions from
        ///     being thrown in generated code. This will allow reading and writing to
        ///     memory outside of the bounds of an array without any runtime checks.
        ///     Disable this check with extreme caution.
        /// </summary>
        ArrayBoundsChecks = 2,

        /// <summary>
        ///     Enable or disable code generation for divide by zero checks.
        ///     Global divide by zero check support is disabled by default when il2cpp.exe
        ///     is launched from the Unity editor.
        ///     Enabling this will cause DivideByZeroException exceptions to be
        ///     thrown in generated code. Most code doesn't need to handle this
        ///     exception, so it is probably safe to leave it disabled.
        /// </summary>
        DivideByZeroChecks = 3
    }

    /// <summary>
    ///     Use this attribute on a class, method, or property to inform the IL2CPP code conversion utility to override the
    ///     global setting for one of a few different runtime checks.
    ///     Example:
    ///     [Il2CppSetOption(Option.NullChecks, false)]
    ///     public static string MethodWithNullChecksDisabled()
    ///     {
    ///     var tmp = new Object();
    ///     return tmp.ToString();
    ///     }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; }
        public object Value  { get; }

        public Il2CppSetOptionAttribute(Option option, object value) {
            this.Option = option;
            this.Value  = value;
        }
    }
}

#if !UNITY_2019_1_OR_NEWER
namespace UnityEngine {
    public sealed class SerializeField : System.Attribute { }
}
namespace JetBrains.Annotations {
    using System;
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
    public sealed class NotNullAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
    public sealed class CanBeNullAttribute : Attribute { }
}
#endif

#if !ODIN_INSPECTOR
namespace Sirenix.OdinInspector {
    using System;
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class ShowInInspectorAttribute : Attribute { }
}
#endif