[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Editor")]
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
        int ID { get; }

        ref T AddComponent<T>() where T : struct, IComponent;
        ref T AddComponent<T>(out bool exist) where T : struct, IComponent;
        bool  AddComponentFast(in int typeId, in int componentId);

        ref T GetComponent<T>() where T : struct, IComponent;
        ref T GetComponent<T>(out bool exist) where T : struct, IComponent;
        int   GetComponentFast(in int typeId);

        void SetComponent<T>(in T value) where T : struct, IComponent;
        bool RemoveComponent<T>() where T : struct, IComponent;
        bool RemoveComponentFast(int typeId, out int cacheIndex);

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
        //todo support hotreload
        [NonSerialized]
        internal World world;

        [SerializeField]
        internal int internalID;
        [SerializeField]
        internal int worldID;

        [SerializeField]
        internal IntDictionary<int> componentsIds;

        [SerializeField]
        private bool isDisposed;

        [SerializeField]
        private int currentArchetypeId;

        [NonSerialized]
        private Archetype currentArchetype;

        [ShowInInspector]
        public int ID => this.internalID;

        internal Entity(int id, int worldID) {
            this.internalID = id;
            this.worldID    = worldID;
            this.world      = World.worlds[this.worldID];

            this.componentsIds = new IntDictionary<int>(Constants.DEFAULT_ENTITY_COMPONENTS_CAPACITY);

            this.currentArchetypeId = 0;

            this.currentArchetype = this.world.archetypes[0];
            this.currentArchetype.Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = this.world.GetCache<T>();

            if (typeInfo.isMarker) {
                const int componentId = -1;
                if (this.componentsIds.Add(typeInfo.id, componentId, out _)) {
                    this.currentArchetype.AddTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
                    return ref cache.Empty();
                }
            }
            else {
                var componentId = cache.Add();
                if (this.componentsIds.Add(typeInfo.id, componentId, out var slotIndex)) {
                    this.currentArchetype.AddTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
                    return ref cache.Get(this.componentsIds.data[slotIndex]);
                }

                cache.Remove(componentId);
            }

#if UNITY_EDITOR
            Debug.LogError("[MORPEH] You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = this.world.GetCache<T>();

            if (typeInfo.isMarker) {
                const int componentId = -1;
                if (this.componentsIds.Add(typeInfo.id, componentId, out _)) {
                    this.currentArchetype.AddTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
                    exist = false;
                    return ref cache.Empty();
                }
            }
            else {
                var componentId = cache.Add();
                if (this.componentsIds.Add(typeInfo.id, componentId, out var slotIndex)) {
                    this.currentArchetype.AddTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
                    exist = false;
                    return ref cache.Get(this.componentsIds.data[slotIndex]);
                }

                cache.Remove(componentId);
            }

#if UNITY_EDITOR
            Debug.LogError("[MORPEH] You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
            exist = true;
            return ref cache.Empty();
        }

        public bool AddComponentFast(in int typeId, in int componentId) {
            if (this.componentsIds.Add(typeId, componentId, out _)) {
                this.currentArchetype.AddTransfer(this.internalID, typeId, out this.currentArchetypeId, out this.currentArchetype);
                return true;
            }

            return false;
        }

        public ref T GetComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = this.world.GetCache<T>();

            if (typeInfo.isMarker) {
                if (this.componentsIds.TryGetIndex(typeInfo.id) >= 0) {
                    return ref cache.Empty();
                }
            }
            else {
                var index = this.componentsIds.TryGetIndex(typeInfo.id);
                if (index >= 0) {
                    return ref cache.Get(this.componentsIds.data[index]);
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
            var cache    = this.world.GetCache<T>();

            if (typeInfo.isMarker) {
                if (this.componentsIds.TryGetIndex(typeInfo.id) >= 0) {
                    exist = true;
                    return ref cache.Empty();
                }
            }
            else {
                var index = this.componentsIds.TryGetIndex(typeInfo.id);
                if (index >= 0) {
                    exist = true;
                    return ref cache.Get(this.componentsIds.data[index]);
                }
            }

            exist = false;
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentFast(in int typeId) => this.componentsIds.GetValue(typeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = this.world.GetCache<T>();

            if (!typeInfo.isMarker) {
                if (this.componentsIds.TryGetValue(typeInfo.id, out var index)) {
                    cache.Set(this.componentsIds.data[index], value);
                }
                else {
                    var componentId = cache.Add(value);
                    this.componentsIds.Add(typeInfo.id, componentId, out _);
                }

                this.currentArchetype.AddTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
            }
            else {
                if (this.componentsIds.Add(typeInfo.id, -1, out _)) {
                    this.currentArchetype.AddTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (this.componentsIds.Remove(typeInfo.id, out var index)) {
                if (typeInfo.isMarker == false) {
                    this.world.GetCache<T>().Remove(index);
                }

                this.currentArchetype.RemoveTransfer(this.internalID, typeInfo.id, out this.currentArchetypeId, out this.currentArchetype);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponentFast(int typeId, out int cacheIndex) {
            if (this.componentsIds.Remove(typeId, out cacheIndex)) {
                this.currentArchetype.RemoveTransfer(this.internalID, typeId, out this.currentArchetypeId, out this.currentArchetype);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Has(int typeID) => this.componentsIds.TryGetIndex(typeID) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has<T>() where T : struct, IComponent {
            var typeID = CacheTypeIdentifier<T>.info.id;
            return this.componentsIds.TryGetIndex(typeID) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDisposed() => this.isDisposed;

        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            var world = this.world;

            var arch = world.archetypes[this.currentArchetypeId];
            arch.Remove(this.internalID);

            foreach (var slotIndex in this.componentsIds) {
                var typeId      = this.componentsIds.slots[slotIndex].key;
                var componentId = this.componentsIds.data[slotIndex];

                if (componentId >= 0) {
                    world.GetCache(typeId)?.Remove(componentId);
                }
            }

            this.DisposeFast();
        }

        internal void DisposeFast() {
            this.componentsIds.Clear();
            this.componentsIds = null;
            this.world         = null;

            this.internalID         = -1;
            this.worldID            = -1;
            this.currentArchetypeId = -1;

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
                        Debug.LogError($"[MORPEH] Can not dispose initializer {initializer.GetType()}");
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
        internal IntDictionary<int> caches;
        [SerializeField]
        internal IntDictionary<int> typedCaches;

        [SerializeField]
        internal List<Archetype> archetypes;
        [SerializeField]
        internal Dictionary<int, List<int>> archetypesByLength;
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
            this.caches            = new IntDictionary<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);
            this.typedCaches       = new IntDictionary<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);

            this.entitiesLength   = 0;
            this.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            this.entities         = new Entity[this.entitiesCapacity];

            this.archetypes = new List<Archetype> {new Archetype(0, new int[0], this.id)};
            this.archetypesByLength = new Dictionary<int, List<int>> {
                [0] = new List<int> {0}
            };
            this.newArchetypes = new List<int>();

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
                    CacheComponents.caches[cache].Dispose();
#if UNITY_EDITOR
                }
                catch (Exception e) {
                    Debug.LogError($"[MORPEH] Can not dispose cache id {cache}");
                    Debug.LogException(e);
                }
#endif
            }

            this.caches.Clear();
            this.caches = null;
            this.typedCaches.Clear();
            this.typedCaches = null;

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

            worlds.Remove(this);
        }
#if UNITY_2019_1_OR_NEWER && !MORPEH_DISABLE_AUTOINITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void InitializationDefaultWorld() {
            CacheComponents.cleanup();

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

        [CanBeNull]
        internal CacheComponents GetCache(int typeId) {
            if (this.caches.TryGetValue(typeId, out var index)) {
                return CacheComponents.caches[index];
            }

            return null;
        }

        public CacheComponents<T> GetCache<T>() where T : struct, IComponent {
            var info = CacheTypeIdentifier<T>.info;
            if (this.typedCaches.TryGetValue(info.id, out var typedIndex)) {
                return CacheComponents<T>.typedCaches[typedIndex];
            }

            CacheComponents<T> cache;
            if (info.isDisposable) {
                var constructedType = typeof(CacheDisposableComponents<>).MakeGenericType(typeof(T));
                cache = (CacheComponents<T>) Activator.CreateInstance(constructedType);
            }
            else {
                cache = new CacheComponents<T>();
            }

            this.caches.Add(info.id, cache.commonCacheId, out _);
            this.typedCaches.Add(info.id, cache.typedCacheId, out _);

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

            foreach (var archetype in this.archetypes) {
                if (archetype.isDirty) {
                    foreach (var filter in archetype.filters) {
                        filter.MakeDirty();
                    }

                    archetype.Swap();

                    archetype.isDirty = false;
                }
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
        public IntHashSet currentEntities;
        [SerializeField]
        public int length;
        [NonSerialized]
        public List<Filter> filters;
        [SerializeField]
        internal IntHashSet addOperations;
        [SerializeField]
        internal IntHashSet removeOperations;
        [SerializeField]
        internal IntDictionary<int> removeTransfer;
        [SerializeField]
        internal IntDictionary<int> addTransfer;
        [SerializeField]
        internal int worldId;
        [SerializeField]
        internal int id;

        //todo support hotreload
        [NonSerialized]
        internal World world;

        internal Archetype(int id, int[] typeIds, int worldId) {
            this.id               = id;
            this.typeIds          = typeIds;
            this.currentEntities  = new IntHashSet();
            this.addOperations    = new IntHashSet();
            this.removeOperations = new IntHashSet();
            this.addTransfer      = new IntDictionary<int>();
            this.removeTransfer   = new IntDictionary<int>();
            this.isDirty          = false;
            this.worldId          = worldId;

            this.Ctor();
        }

        internal void Ctor() {
            this.world   = World.worlds[this.worldId];
            this.filters = new List<Filter>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int entityId) {
            this.addOperations.Add(entityId);
            this.removeOperations.Remove(entityId);
            this.isDirty = true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int entityId) {
            this.removeOperations.Add(entityId);
            this.addOperations.Remove(entityId);
            this.isDirty = true;
        }
        
        public void AddFilter(Filter filter) {
            this.filters.Add(filter);
            this.isDirty = true;
        }

        public void RemoveFilter(Filter filter) {
            if (this.filters.Remove(filter)) {
                this.isDirty = true;
            }
        }

        public void Swap() {
            foreach (var entityId in this.removeOperations) {
                this.currentEntities.Remove(entityId);
            }

            this.removeOperations.Clear();
            foreach (var entityId in this.addOperations) {
                this.currentEntities.Add(entityId);
            }

            this.addOperations.Clear();

            this.length  = this.currentEntities.count;
            this.isDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTransfer(int entityId, int typeId, out int archetypeId, out Archetype archetype) {
            this.Remove(entityId);

            if (this.addTransfer.TryGetValue(typeId, out archetypeId)) {
                archetype = this.world.archetypes[archetypeId];
                archetype.Add(entityId);
            }
            else {
                archetype = this.world.GetArchetype(this.typeIds, typeId, true, out archetypeId);
                archetype.Add(entityId);

                this.addTransfer.Add(typeId, archetypeId, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveTransfer(int entityId, int typeId, out int archetypeId, out Archetype archetype) {
            this.Remove(entityId);

            if (this.removeTransfer.TryGetValue(typeId, out archetypeId)) {
                archetype = this.world.archetypes[archetypeId];
                archetype.Add(entityId);
            }
            else {
                archetype = this.world.GetArchetype(this.typeIds, typeId, false, out archetypeId);
                archetype.Add(entityId);

                this.removeTransfer.Add(typeId, archetypeId, out _);
            }
        }

        public void Dispose() {
            this.isDirty = false;

            this.typeIds = null;
            this.world   = null;

            this.currentEntities.Clear();
            this.currentEntities = null;
            this.addOperations.Clear();
            this.addOperations = null;
            this.removeOperations.Clear();
            this.removeOperations = null;
            
            this.addTransfer.Clear();
            this.addTransfer = null;

            this.removeTransfer.Clear();
            this.removeTransfer = null;
        }
    }

    internal struct Modification {
        public int entityId;
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
                arch.currentEntities.CopyTo(this.entitiesCacheForBags);
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
                    this.MakeDirty();
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

        [Obsolete("Use Singleton Asset")]
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
            private static readonly int  typeId;
            private static readonly bool isMarker;

            private CacheComponents<T> cacheComponents;
            private IntDictionary<T>   sharedComponents;
            private int[]              ids;
            private World              world;

            static ComponentsBag() {
                var info = CacheTypeIdentifier<T>.info;
                isMarker = info.isMarker;
                typeId   = info.id;

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
                    this.ids[i] = this.world.entities[entities[i]].componentsIds.TryGetIndex(typeId);
                }

                this.sharedComponents = this.cacheComponents.components;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T GetComponent(in int index) => ref this.sharedComponents.data[this.ids[index]];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetComponent(in int index, in T value) => this.sharedComponents.data[this.ids[index]] = value;

            internal static ref ComponentsBag<T> Get(in int index) => ref cache[index];

            internal static int Create(World world) {
                var worldCache = world.GetCache<T>();

                var bag = new ComponentsBag<T> {
                    world            = world,
                    ids              = new int[1],
                    cacheComponents  = worldCache,
                    sharedComponents = worldCache.components
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
            private readonly World           world;
            private readonly List<Archetype> archetypes;
            
            private Entity          current;

            private int archetypeId;
            private int archetypeCount;
            private IntHashSet.Enumerator archetypeEnumerator;

            internal EntityEnumerator(World world, List<Archetype> archetypes) {
                this.world      = world;
                this.archetypes = archetypes;
                this.current    = null;
                
                this.archetypeId = 0;
                this.archetypeCount  = this.archetypes.Count;
                this.archetypeEnumerator = this.archetypeCount > 0 ? this.archetypes[0].currentEntities.GetEnumerator() : default;
            }

            public bool MoveNext() {
                if (this.archetypeId < this.archetypeCount) {
                    var move = this.archetypeEnumerator.MoveNext();
                    if (move) {
                        this.current = this.world.entities[this.archetypeEnumerator.Current];
                        return true;
                    }
                    
                    while (++this.archetypeId < this.archetypeCount) {
                        this.archetypeEnumerator = this.archetypes[this.archetypeId].currentEntities.GetEnumerator();
                        move = this.archetypeEnumerator.MoveNext();
                        if (move) {
                            this.current = this.world.entities[this.archetypeEnumerator.Current];
                            return true;
                        }
                    }
                }

                return false;
            }

            public void Reset() {
                this.current = null;
                this.archetypeId  = 0;
                this.archetypeEnumerator = this.archetypeCount > 0 ? this.archetypes[0].currentEntities.GetEnumerator() : default;
            }

            public IEntity Current => this.current;

            object IEnumerator.Current => this.current;

            public void Dispose() {
                this.archetypeCount  = -1;
                this.archetypeId = -1;
                this.archetypeEnumerator.Dispose();

                this.current = null;
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
                getBoxed = (world, componentId) => world.GetCache<T>().components.data[componentId],
                setBoxed = (world, componentId, value) => world.GetCache<T>().components.data[componentId] = (T) value,
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
    public abstract class CacheComponents : IDisposable {
        internal static List<CacheComponents> caches  = new List<CacheComponents>();
        internal static Action                cleanup = () => caches.Clear();

        [SerializeField]
        internal int commonCacheId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void Remove(in int id);

        public abstract void Dispose();
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public class CacheComponents<T> : CacheComponents where T : struct, IComponent {
        internal static List<CacheComponents<T>> typedCaches = new List<CacheComponents<T>>();

        [SerializeField]
        internal IntDictionary<T> components;
        [SerializeField]
        internal IntStack freeIndexes;
        [SerializeField]
        internal int typedCacheId;
        [SerializeField]
        internal int typedId;

        static CacheComponents() {
            cleanup += () => typedCaches.Clear();
        }

        internal CacheComponents() {
            this.typedId = CacheTypeIdentifier<T>.info.id;

            this.components  = new IntDictionary<T>(Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY);
            this.freeIndexes = new IntStack();

            this.components.Add(0, default, out _);

            this.typedCacheId = typedCaches.Count;
            typedCaches.Add(this);

            this.commonCacheId = caches.Count;
            caches.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Add() {
            if (this.freeIndexes.size > 0) {
                return this.freeIndexes.Pop();
            }

            this.components.Add(this.components.count, default, out var index);
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Add(in T value) {
            int index;
            if (this.freeIndexes.size > 0) {
                index = this.freeIndexes.Pop();

                this.components.data[index] = value;
                return index;
            }

            this.components.Add(this.components.count, value, out index);
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(in IEntity entity) {
            var componentId = this.Add();
            if (entity.AddComponentFast(this.typedId, componentId)) {
                return ref this.components.data[componentId];
            }

            this.Remove(componentId);
            return ref this.components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddComponent(in IEntity entity, in T value) {
            var componentId = this.Add(value);
            if (entity.AddComponentFast(this.typedId, componentId)) {
                return true;
            }

            this.Remove(componentId);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Get(in int id) => ref this.components.data[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(in IEntity entity) => ref this.components.data[entity.GetComponentFast(this.typedId)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Set(in int id, in T value) => this.components.data[id] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent(in IEntity entity, in T value) => this.components.data[entity.GetComponentFast(this.typedId)] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.components.data[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Remove(in int id) {
            this.components.data[id] = default;
            this.freeIndexes.Push(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent(in IEntity entity) {
            if (entity.RemoveComponentFast(this.typedId, out var cacheIndex)) {
                this.Remove(cacheIndex);
            }
        }

        public override void Dispose() {
            this.components = null;
            this.freeIndexes.Clear();
            this.freeIndexes = null;
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal sealed class CacheDisposableComponents<T> : CacheComponents<T> where T : struct, IComponent, IDisposable {
        internal override void Remove(in int id) {
            this.components.data[id].Dispose();
            base.Remove(in id);
        }

        public override void Dispose() {
            for (int i = 0, length = this.components.slots.Length; i < length; i++) {
                ref var slot = ref this.components.slots[i];
                if (slot.key != -1) {
                    this.components.data[i].Dispose();
                }
            }

            base.Dispose();
        }
    }

    namespace Utils {
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public static class ListExtensions {
            public static void RemoveAtFast<T>(this IList<T> list, int index) {
                var count = list.Count - 1;
                list[index] = list[count];
                list.RemoveAt(count);
            }
        }

        [Serializable]
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed class IntHashSet : IEnumerable<int> {
            public int    count;
            public int[]  buckets;
            public Slot[] slots;

            public int lastIndex;
            public int freeList;

            public IntHashSet(in int capacity = 0) {
                this.lastIndex = 0;
                this.count     = 0;
                this.freeList  = -1;

                var prime = HashHelpers.GetPrime(capacity);
                this.buckets = new int[prime];
                this.slots   = new Slot[prime];

                for (var i = 0; i < prime; i++) {
                    this.slots[i].value = -1;
                }
            }

            public bool Add(in int value) {
                HashHelpers.DivRem(value, this.buckets.Length, out var rem);

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].value == value) {
                        return false;
                    }
                }

                int newIndex;
                if (this.freeList >= 0) {
                    newIndex      = this.freeList;
                    this.freeList = this.slots[newIndex].next;
                }
                else {
                    if (this.lastIndex == this.slots.Length) {
                        var newSize = HashHelpers.ExpandPrime(this.count);

                        Array.Resize(ref this.slots, newSize);

                        var numArray = new int[newSize];

                        for (int i = 0, length = this.lastIndex; i < length; ++i) {
                            ref var slot = ref this.slots[i];
                            HashHelpers.DivRem(slot.value, newSize, out var newResizeIndex);

                            slot.next = numArray[newResizeIndex] - 1;

                            numArray[newResizeIndex] = i + 1;
                        }

                        for (int i = this.lastIndex + 1, length = newSize; i < length; i++) {
                            this.slots[i].value = -1;
                        }

                        this.buckets = numArray;

                        HashHelpers.DivRem(value, this.buckets.Length, out rem);
                    }

                    newIndex = this.lastIndex;
                    ++this.lastIndex;
                }

                ref var newSlot = ref this.slots[newIndex];

                newSlot.value = value;
                newSlot.next  = this.buckets[rem] - 1;

                this.buckets[rem] = newIndex + 1;

                ++this.count;
                return true;
            }

            public bool Remove(in int value) {
                HashHelpers.DivRem(value, this.buckets.Length, out var rem);

                var num = -1;
                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].value == value) {
                        if (num < 0) {
                            this.buckets[rem] = this.slots[i].next + 1;
                        }
                        else {
                            this.slots[num].next = this.slots[i].next;
                        }

                        this.slots[i].value = -1;
                        this.slots[i].next  = this.freeList;
                        --this.count;
                        if (this.count == 0) {
                            this.lastIndex = 0;
                            this.freeList  = -1;
                        }
                        else {
                            this.freeList = i;
                        }

                        return true;
                    }

                    num = i;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int[] array) {
                int num = 0;
                for (int i = 0, li = this.lastIndex, length = this.count; i < li && num < length; ++i) {
                    if (this.slots[i].value < 0) {
                        continue;
                    }

                    array[num] = this.slots[i].value;
                    ++num;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear() {
                if (this.lastIndex <= 0) {
                    return;
                }

                Array.Clear(this.slots, 0, this.lastIndex);
                Array.Clear(this.buckets, 0, this.buckets.Length);
                this.lastIndex = 0;
                this.count     = 0;
                this.freeList  = -1;
            }

            public Enumerator GetEnumerator() => new Enumerator(this);

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);


            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Slot {
                public int value;
                public int next;
            }

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Enumerator : IEnumerator<int> {
                private readonly IntHashSet set;

                private int index;
                private int current;

                internal Enumerator(IntHashSet set) {
                    this.set     = set;
                    this.index   = 0;
                    this.current = default;
                }

                public bool MoveNext() {
                    for (; this.index < this.set.lastIndex; ++this.index) {
                        ref var slot = ref this.set.slots[this.index];
                        if (slot.value < 0) {
                            continue;
                        }

                        this.current = slot.value;
                        ++this.index;

                        return true;
                    }

                    this.index   = this.set.lastIndex + 1;
                    this.current = default;
                    return false;
                }

                public int Current => this.current;

                object IEnumerator.Current => this.current;

                void IEnumerator.Reset() {
                    this.index   = 0;
                    this.current = default;
                }

                public void Dispose() {
                }
            }
        }

        [Serializable]
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed class IntDictionary<T> : IEnumerable<int> {
            public int    count;
            public int[]  buckets;
            public Slot[] slots;
            public T[]    data;

            public int lastIndex;
            public int freeList;

            public IntDictionary(in int capacity = 0) {
                this.lastIndex = 0;
                this.count     = 0;
                this.freeList  = -1;

                var prime = HashHelpers.GetPrime(capacity);
                this.buckets = new int[prime];
                this.slots   = new Slot[prime];
                this.data    = new T[prime];

                for (var i = 0; i < prime; i++) {
                    this.slots[i].key = -1;
                }
            }

            public bool Add(in int key, in T value, out int slotIndex) {
                HashHelpers.DivRem(key, this.buckets.Length, out var rem);

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key == key) {
                        slotIndex = -1;
                        return false;
                    }
                }

                if (this.freeList >= 0) {
                    slotIndex     = this.freeList;
                    this.freeList = this.slots[slotIndex].next;
                }
                else {
                    if (this.lastIndex == this.slots.Length) {
                        var newSize = HashHelpers.ExpandPrime(this.count);

                        Array.Resize(ref this.slots, newSize);
                        Array.Resize(ref this.data, newSize);

                        var numArray = new int[newSize];

                        for (int i = 0, length = this.lastIndex; i < length; ++i) {
                            ref var slot = ref this.slots[i];
                            HashHelpers.DivRem(slot.key, newSize, out var newResizeIndex);
                            slot.next = numArray[newResizeIndex] - 1;

                            numArray[newResizeIndex] = i + 1;
                        }

                        for (int i = this.lastIndex + 1, length = newSize; i < length; i++) {
                            this.slots[i].key = -1;
                        }

                        this.buckets = numArray;

                        HashHelpers.DivRem(key, this.buckets.Length, out rem);
                    }

                    slotIndex = this.lastIndex;
                    ++this.lastIndex;
                }

                ref var newSlot = ref this.slots[slotIndex];

                newSlot.key  = key;
                newSlot.next = this.buckets[rem] - 1;

                this.data[slotIndex] = value;

                this.buckets[rem] = slotIndex + 1;

                ++this.count;
                return true;
            }

            public bool Remove(in int key, [CanBeNull] out T lastValue) {
                HashHelpers.DivRem(key, this.buckets.Length, out var rem);

                var num = -1;
                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key == key) {
                        if (num < 0) {
                            this.buckets[rem] = this.slots[i].next + 1;
                        }
                        else {
                            this.slots[num].next = this.slots[i].next;
                        }

                        ref var removedSlot = ref this.slots[i];

                        lastValue    = this.data[i];
                        this.data[i] = default;

                        removedSlot.key  = -1;
                        removedSlot.next = this.freeList;

                        --this.count;
                        if (this.count == 0) {
                            this.lastIndex = 0;
                            this.freeList  = -1;
                        }
                        else {
                            this.freeList = i;
                        }

                        return true;
                    }

                    num = i;
                }

                lastValue = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in int key, [CanBeNull] out T value) {
                HashHelpers.DivRem(key, this.buckets.Length, out var rem);

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key == key) {
                        value = this.data[i];
                        return true;
                    }
                }

                value = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T GetValue(in int key) {
                HashHelpers.DivRem(key, this.buckets.Length, out var rem);

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key == key) {
                        return this.data[i];
                    }
                }

                return default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TryGetIndex(in int key) {
                HashHelpers.DivRem(key, this.buckets.Length, out var rem);

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key == key) {
                        return i;
                    }
                }

                return -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] array) {
                int num = 0;
                for (int i = 0, li = this.lastIndex, length = this.count; i < li && num < length; ++i) {
                    if (this.slots[i].key < 0) {
                        continue;
                    }

                    array[num] = this.data[i];
                    ++num;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear() {
                if (this.lastIndex <= 0) {
                    return;
                }

                Array.Clear(this.slots, 0, this.lastIndex);
                Array.Clear(this.buckets, 0, this.buckets.Length);
                this.lastIndex = 0;
                this.count     = 0;
                this.freeList  = -1;
            }

            public Enumerator GetEnumerator() => new Enumerator(this);

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Slot {
                public int key;
                public int next;
            }

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Enumerator : IEnumerator<int> {
                private readonly IntDictionary<T> dictionary;

                private int index;
                private int current;

                internal Enumerator(IntDictionary<T> dictionary) {
                    this.dictionary = dictionary;
                    this.index      = 0;
                    this.current    = default;
                }

                public bool MoveNext() {
                    for (; this.index < this.dictionary.lastIndex; ++this.index) {
                        ref var slot = ref this.dictionary.slots[this.index];
                        if (slot.key < 0) {
                            continue;
                        }

                        this.current = this.index;
                        ++this.index;

                        return true;
                    }

                    this.index   = this.dictionary.lastIndex + 1;
                    this.current = default;
                    return false;
                }

                public int Current => this.current;

                object IEnumerator.Current => this.current;

                void IEnumerator.Reset() {
                    this.index   = 0;
                    this.current = default;
                }

                public void Dispose() {
                }
            }
        }

        [Serializable]
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed class IntStack {
            public int[] array;
            public int   size;
            public int   lastSize;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(in int value) {
                if (this.size == this.lastSize) {
                    Array.Resize(ref this.array, this.lastSize = this.lastSize == 0 ? 4 : this.lastSize << 1);
                }

                this.array[this.size++] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Pop() => this.array[--this.size];

            public void Clear() {
                this.array = null;
                this.size  = this.lastSize = 0;
            }
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        internal static class HashHelpers {
            internal static readonly int[] primes = {
                3,
                7,
                11,
                17,
                23,
                29,
                37,
                47,
                59,
                71,
                89,
                107,
                131,
                163,
                197,
                239,
                293,
                353,
                431,
                521,
                631,
                761,
                919,
                1103,
                1327,
                1597,
                1931,
                2333,
                2801,
                3371,
                4049,
                4861,
                5839,
                7013,
                8419,
                10103,
                12143,
                14591,
                17519,
                21023,
                25229,
                30293,
                36353,
                43627,
                52361,
                62851,
                75431,
                90523,
                108631,
                130363,
                156437,
                187751,
                225307,
                270371,
                324449,
                389357,
                467237,
                560689,
                672827,
                807403,
                968897,
                1162687,
                1395263,
                1674319,
                2009191,
                2411033,
                2893249,
                3471899,
                4166287,
                4999559,
                5999471,
                7199369
            };

            internal static bool IsPrime(int candidate) {
                if ((candidate & 1) == 0) {
                    return candidate == 2;
                }

                var num = Sqrt(candidate);
                for (var index = 3; index <= num; index += 2) {
                    if (candidate % index == 0) {
                        return false;
                    }
                }

                return true;
            }

            public static int ExpandPrime(int oldSize) {
                var min = oldSize << 1;
                return min > 2146435069U && 2146435069 > oldSize ? 2146435069 : GetPrime(min);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int DivRem(int left, int right, out int result) {
                var div = left / right;
                result = left - div * right;
                return div;
            }

            //todo possible refactor?
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Sqrt(int num) {
                if (0 == num) {
                    return 0;
                }

                var n  = num / 2 + 1;
                var n1 = (n + num / n) / 2;
                while (n1 < n) {
                    n  = n1;
                    n1 = (n + num / n) / 2;
                }

                return n;
            }

            public static int GetPrime(int min) {
                for (int index = 0, length = primes.Length; index < length; ++index) {
                    var prime = primes[index];
                    if (prime >= min) {
                        return prime;
                    }
                }

                for (int candidate = min | 1, length = int.MaxValue; candidate < length; candidate += 2) {
                    if (HashHelpers.IsPrime(candidate) && (candidate - 1) % 101 != 0)
                        return candidate;
                }

                return min;
            }
        }
    }
}

namespace Unity.IL2CPP.CompilerServices {
    using System;

    public enum Option {
        NullChecks         = 1,
        ArrayBoundsChecks  = 2,
        DivideByZeroChecks = 3
    }

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