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

    public interface IComponent {
    }

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

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class Entity {
        //todo support hotreload
        [NonSerialized]
        internal World world;

        [SerializeField]
        internal int internalID;
        [SerializeField]
        internal int worldID;

        [SerializeField]
        internal UnsafeIntHashMap<int> componentsIds;

        [SerializeField]
        internal bool isDirty;
        [SerializeField]
        internal bool isDisposed;

        [SerializeField]
        internal int previousArchetypeId;
        [SerializeField]
        internal int currentArchetypeId;
        [SerializeField]
        internal int indexInCurrentArchetype;

        [NonSerialized]
        internal Archetype currentArchetype;

        [ShowInInspector]
        public int ID => this.internalID;

        internal Entity(int id, int worldID) {
            this.internalID = id;
            this.worldID    = worldID;
            this.world      = World.worlds.data[this.worldID];

            this.componentsIds = new UnsafeIntHashMap<int>(Constants.DEFAULT_ENTITY_COMPONENTS_CAPACITY);

            this.indexInCurrentArchetype = -1;
            this.previousArchetypeId     = -1;
            this.currentArchetypeId      = 0;

            this.currentArchetype = this.world.archetypes.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            var arch = this.world.archetypes.data[this.currentArchetypeId];
            arch.Remove(this);

            foreach (var slotIndex in this.componentsIds) {
                var typeId      = this.componentsIds.GetKeyByIndex(slotIndex);
                var componentId = this.componentsIds.GetValueByIndex(slotIndex + 1);

                if (componentId >= 0) {
                    this.world.GetCache(typeId)?.Remove(componentId);
                }
            }

            this.DisposeFast();
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public static class EntityExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = entity.world.GetCache<T>();

            if (typeInfo.isMarker) {
                const int componentId = -1;
                if (entity.componentsIds.Add(typeInfo.id, componentId, out _)) {
                    entity.AddTransfer(typeInfo.id);
                    return ref cache.Empty();
                }
            }
            else {
                var componentId = cache.Add();
                if (entity.componentsIds.Add(typeInfo.id, componentId, out var slotIndex)) {
                    entity.AddTransfer(typeInfo.id);
                    return ref cache.Get(entity.componentsIds.GetValueByIndex(slotIndex));
                }

                cache.Remove(componentId);
            }

#if UNITY_EDITOR
            Debug.LogError("[MORPEH] You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = entity.world.GetCache<T>();

            if (typeInfo.isMarker) {
                const int componentId = -1;
                if (entity.componentsIds.Add(typeInfo.id, componentId, out _)) {
                    entity.AddTransfer(typeInfo.id);
                    exist = false;
                    return ref cache.Empty();
                }
            }
            else {
                var componentId = cache.Add();
                if (entity.componentsIds.Add(typeInfo.id, componentId, out var slotIndex)) {
                    entity.AddTransfer(typeInfo.id);
                    exist = false;
                    return ref cache.Get(entity.componentsIds.GetValueByIndex(slotIndex));
                }

                cache.Remove(componentId);
            }

#if UNITY_EDITOR
            Debug.LogError("[MORPEH] You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
            exist = true;
            return ref cache.Empty();
        }

        public static bool AddComponentFast(this Entity entity, in int typeId, in int componentId) {
            if (entity.componentsIds.Add(typeId, componentId, out _)) {
                entity.AddTransfer(typeId);
                return true;
            }

            return false;
        }

        public static ref T GetComponent<T>(this Entity entity) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = entity.world.GetCache<T>();

            if (typeInfo.isMarker) {
                if (entity.componentsIds.TryGetIndex(typeInfo.id) >= 0) {
                    return ref cache.Empty();
                }
            }
            else {
                var index = entity.componentsIds.TryGetIndex(typeInfo.id);
                if (index >= 0) {
                    return ref cache.Get(entity.componentsIds.GetValueByIndex(index));
                }
            }

#if UNITY_EDITOR
            Debug.LogError("[MORPEH] You're trying to get a component that doesn't exists!");
#endif
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = entity.world.GetCache<T>();

            if (typeInfo.isMarker) {
                if (entity.componentsIds.TryGetIndex(typeInfo.id) >= 0) {
                    exist = true;
                    return ref cache.Empty();
                }
            }
            else {
                var index = entity.componentsIds.TryGetIndex(typeInfo.id);
                if (index >= 0) {
                    exist = true;
                    return ref cache.Get(entity.componentsIds.GetValueByIndex(index));
                }
            }

            exist = false;
            return ref cache.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetComponentFast(this Entity entity, in int typeId) => entity.componentsIds.GetValueByKey(typeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, in T value) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            var cache    = entity.world.GetCache<T>();

            if (!typeInfo.isMarker) {
                if (entity.componentsIds.TryGetValue(typeInfo.id, out var index)) {
                    cache.Set(entity.componentsIds.GetValueByIndex(index), value);
                }
                else {
                    var componentId = cache.Add(value);
                    entity.componentsIds.Add(typeInfo.id, componentId, out _);
                }

                entity.AddTransfer(typeInfo.id);
            }
            else {
                if (entity.componentsIds.Add(typeInfo.id, -1, out _)) {
                    entity.AddTransfer(typeInfo.id);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponent<T>(this Entity entity) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (entity.componentsIds.Remove(typeInfo.id, out var index)) {
                if (entity.componentsIds.length == 0) {
                    entity.world.RemoveEntity(entity);
                    return true;
                }

                if (typeInfo.isMarker == false) {
                    entity.world.GetCache<T>().Remove(index);
                }

                entity.RemoveTransfer(typeInfo.id);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponentFast(this Entity entity, int typeId, out int cacheIndex) {
            if (entity.componentsIds.Remove(typeId, out cacheIndex)) {
                if (entity.componentsIds.length == 0) {
                    entity.world.RemoveEntity(entity);
                    return true;
                }

                entity.RemoveTransfer(typeId);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this Entity entity, int typeID) => entity.componentsIds.TryGetIndex(typeID) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : struct, IComponent {
            var typeID = CacheTypeIdentifier<T>.info.id;
            return entity.componentsIds.TryGetIndex(typeID) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddTransfer(this Entity entity, int typeId) {
            if (entity.previousArchetypeId == -1) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetype.AddTransfer(typeId, out entity.currentArchetypeId, out entity.currentArchetype);
            if (entity.isDirty == true) {
                return;
            }

            entity.world.dirtyEntities.Add(entity);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveTransfer(this Entity entity, int typeId) {
            if (entity.previousArchetypeId == -1) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetype.RemoveTransfer(typeId, out entity.currentArchetypeId, out entity.currentArchetype);
            if (entity.isDirty == true) {
                return;
            }

            entity.world.dirtyEntities.Add(entity);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransfer(this Entity entity) {
            if (entity.previousArchetypeId > 0 && entity.indexInCurrentArchetype >= 0) {
                entity.world.archetypes.data[entity.previousArchetypeId].Remove(entity);
            }

            entity.previousArchetypeId = -1;
            entity.currentArchetype.Add(entity, out entity.indexInCurrentArchetype);
            entity.isDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DisposeFast(this Entity entity) {
            entity.componentsIds.Clear();
            entity.componentsIds = null;
            entity.world         = null;

            entity.internalID         = -1;
            entity.worldID            = -1;
            entity.currentArchetypeId = -1;

            entity.isDisposed = true;
        }

        public static bool IsNullOrDisposed([CanBeNull] this Entity entity) => entity == null || entity.isDisposed;
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class SystemsGroup : IDisposable {
        [ShowInInspector]
        internal FastList<ISystem> systems;
        [ShowInInspector]
        internal FastList<ISystem> fixedSystems;
        [ShowInInspector]
        internal FastList<ISystem> lateSystems;

        [ShowInInspector]
        internal FastList<ISystem> disabledSystems;
        [ShowInInspector]
        internal FastList<ISystem> disabledFixedSystems;
        [ShowInInspector]
        internal FastList<ISystem> disabledLateSystems;

        [ShowInInspector]
        internal FastList<IInitializer> newInitializers;
        [ShowInInspector]
        internal FastList<IInitializer> initializers;
        [ShowInInspector]
        internal FastList<IDisposable> disposables;
        internal World  world;
        internal Action delayedAction;

        private SystemsGroup() {
        }

        internal SystemsGroup(World world) {
            this.world         = world;
            this.delayedAction = null;

            this.systems      = new FastList<ISystem>();
            this.fixedSystems = new FastList<ISystem>();
            this.lateSystems  = new FastList<ISystem>();

            this.disabledSystems      = new FastList<ISystem>();
            this.disabledFixedSystems = new FastList<ISystem>();
            this.disabledLateSystems  = new FastList<ISystem>();

            this.newInitializers = new FastList<IInitializer>();
            this.initializers    = new FastList<IInitializer>();
            this.disposables     = new FastList<IDisposable>();
        }

        public void Dispose() {
            if (this.disposables == null) {
                return;
            }

            void DisposeSystems(FastList<ISystem> systemsToDispose) {
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

            if (this.newInitializers.length > 0) {
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
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public static class SystemsGroupExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize(this SystemsGroup systemsGroup) {
            if (systemsGroup.disposables.length > 0) {
                foreach (var disposable in systemsGroup.disposables) {
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

                systemsGroup.disposables.Clear();
            }

            systemsGroup.world.UpdateFilters();
            if (systemsGroup.newInitializers.length > 0) {
                foreach (var initializer in systemsGroup.newInitializers) {
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

                    systemsGroup.world.UpdateFilters();
                    systemsGroup.initializers.Add(initializer);
                }

                systemsGroup.newInitializers.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(this SystemsGroup systemsGroup, float deltaTime) {
#if UNITY_EDITOR
            systemsGroup.delayedAction = null;
#endif
            systemsGroup.Initialize();
            for (int i = 0, length = systemsGroup.systems.length; i < length; i++) {
                var system = systemsGroup.systems.data[i];
#if UNITY_EDITOR
                try {
                    system.OnUpdate(deltaTime);
                }
                catch (Exception e) {
                    systemsGroup.SystemThrowException(system, e);
                }
#else
                system.OnUpdate(deltaTime);
#endif
                systemsGroup.world.UpdateFilters();
            }
#if UNITY_EDITOR
            systemsGroup.delayedAction?.Invoke();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FixedUpdate(this SystemsGroup systemsGroup, float deltaTime) {
#if UNITY_EDITOR
            systemsGroup.delayedAction = null;
#endif
            for (int i = 0, length = systemsGroup.fixedSystems.length; i < length; i++) {
                var system = systemsGroup.fixedSystems.data[i];
#if UNITY_EDITOR
                try {
                    system.OnUpdate(deltaTime);
                }
                catch (Exception e) {
                    systemsGroup.SystemThrowException(system, e);
                }
#else
                system.OnUpdate(deltaTime);
#endif
                systemsGroup.world.UpdateFilters();
            }
#if UNITY_EDITOR
            systemsGroup.delayedAction?.Invoke();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LateUpdate(this SystemsGroup systemsGroup, float deltaTime) {
#if UNITY_EDITOR
            systemsGroup.delayedAction = null;
#endif
            systemsGroup.world.UpdateFilters();

            for (int i = 0, length = systemsGroup.lateSystems.length; i < length; i++) {
                var system = systemsGroup.lateSystems.data[i];
#if UNITY_EDITOR
                try {
                    system.OnUpdate(deltaTime);
                }
                catch (Exception e) {
                    systemsGroup.SystemThrowException(system, e);
                }
#else
                system.OnUpdate(deltaTime);
#endif
                systemsGroup.world.UpdateFilters();
            }
#if UNITY_EDITOR
            systemsGroup.delayedAction?.Invoke();
#endif
        }


#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SystemThrowException(this SystemsGroup systemsGroup, ISystem system, Exception exception) {
            Debug.LogError($"[MORPEH] Can not update {system.GetType()}. System will be disabled.");
            Debug.LogException(exception);
            systemsGroup.delayedAction += () => systemsGroup.DisableSystem(system);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInitializer<T>(this SystemsGroup systemsGroup, T initializer) where T : class, IInitializer {
            initializer.World = systemsGroup.world;

            systemsGroup.newInitializers.Add(initializer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveInitializer<T>(this SystemsGroup systemsGroup, T initializer) where T : class, IInitializer {
            var index = systemsGroup.newInitializers.IndexOf(initializer);
            if (index >= 0) {
                systemsGroup.newInitializers.RemoveAt(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddSystem<T>(this SystemsGroup systemsGroup, T system, bool enabled = true) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            if (enabled && collection.IndexOf(system) < 0) {
                collection.Add(system);
                systemsGroup.AddInitializer(system);
                return true;
            }

            if (!enabled && disabledCollection.IndexOf(system) < 0) {
                disabledCollection.Add(system);
                systemsGroup.AddInitializer(system);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EnableSystem<T>(this SystemsGroup systemsGroup, T system) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            var index = disabledCollection.IndexOf(system);
            if (index >= 0) {
                collection.Add(system);
                disabledCollection.RemoveAt(index);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DisableSystem<T>(this SystemsGroup systemsGroup, T system) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            var index = collection.IndexOf(system);
            if (index >= 0) {
                disabledCollection.Add(system);
                collection.RemoveAt(index);
                return true;
            }

            return false;
        }

        public static bool RemoveSystem<T>(this SystemsGroup systemsGroup, T system) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            var index = collection.IndexOf(system);
            if (index >= 0) {
                collection.RemoveAt(index);
                systemsGroup.disposables.Add(system);
                systemsGroup.RemoveInitializer(system);
                return true;
            }

            index = disabledCollection.IndexOf(system);
            if (index >= 0) {
                disabledCollection.RemoveAt(index);
                systemsGroup.disposables.Add(system);
                systemsGroup.RemoveInitializer(system);
                return true;
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
        public static World Default => worlds.data[0];
        [NotNull]
        internal static FastList<World> worlds = new FastList<World> {null};

        [NonSerialized]
        public Filter Filter;
        [SerializeField]
        public bool UpdateByUnity;

        [NonSerialized]
        internal FastList<Filter> filters;

        //todo custom collection
        [ShowInInspector]
        [NonSerialized]
        internal SortedList<int, SystemsGroup> systemsGroups;

        //todo custom collection
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

        [NonSerialized]
        internal FastList<Entity> dirtyEntities;

        [SerializeField]
        private IntFastList freeEntityIDs;
        [SerializeField]
        private IntFastList nextFreeEntityIDs;

        [SerializeField]
        internal UnsafeIntHashMap<int> caches;
        [SerializeField]
        internal UnsafeIntHashMap<int> typedCaches;

        [SerializeField]
        internal FastList<Archetype> archetypes;
        [SerializeField]
        internal IntHashMap<IntFastList> archetypesByLength;
        [SerializeField]
        internal IntFastList newArchetypes;
        [NonSerialized]
        private IntFastList archetypeCache;

        [SerializeField]
        internal int worldId;

        public static World Create() => new World().Initialize();

        private World() {
            this.Ctor();
        }

        internal void Ctor() {
            this.systemsGroups    = new SortedList<int, SystemsGroup>();
            this.newSystemsGroups = new SortedList<int, SystemsGroup>();

            this.Filter         = new Filter(this);
            this.filters        = new FastList<Filter>();
            this.archetypeCache = new IntFastList();

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
            this.worldId           = worlds.length - 1;
            this.dirtyEntities     = new FastList<Entity>();
            this.freeEntityIDs     = new IntFastList();
            this.nextFreeEntityIDs = new IntFastList();
            this.caches            = new UnsafeIntHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);
            this.typedCaches       = new UnsafeIntHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);

            this.entitiesLength   = 0;
            this.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            this.entities         = new Entity[this.entitiesCapacity];

            this.archetypes         = new FastList<Archetype> {new Archetype(0, new int[0], this.worldId)};
            this.archetypesByLength = new IntHashMap<IntFastList>();
            this.archetypesByLength.Add(0, new IntFastList {0}, out _);
            this.newArchetypes = new IntFastList();

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

            foreach (var cache in this.caches) {
#if UNITY_EDITOR
                try {
#endif
                    ComponentsCache.caches.data[cache].Dispose();
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

            foreach (var index in this.archetypesByLength) {
                this.archetypesByLength.GetValueByIndex(index).Clear();
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
            ComponentsCache.cleanup();

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
            var typesLength = this.archetypeCache.length;

            if (this.archetypesByLength.TryGetValue(typesLength, out var archsl)) {
                for (var index = 0; index < archsl.length; index++) {
                    archetypeId = archsl.Get(index);
                    archetype   = this.archetypes.data[archetypeId];
                    var check = true;
                    for (int i = 0, length = typesLength; i < length; i++) {
                        if (archetype.typeIds[i] != this.archetypeCache.Get(i)) {
                            check = false;
                            break;
                        }
                    }

                    if (check) {
                        return archetype;
                    }
                }
            }

            archetypeId = this.archetypes.length;
            var newArchetype = new Archetype(archetypeId, this.archetypeCache.ToArray(), this.worldId);
            this.archetypes.Add(newArchetype);
            if (this.archetypesByLength.TryGetValue(typesLength, out archsl)) {
                archsl.Add(archetypeId);
            }
            else {
                this.archetypesByLength.Add(typesLength, new IntFastList {archetypeId}, out _);
            }

            this.newArchetypes.Add(archetypeId);

            archetype = newArchetype;

            return archetype;
        }

        [CanBeNull]
        internal ComponentsCache GetCache(int typeId) {
            if (this.caches.TryGetValue(typeId, out var index)) {
                return ComponentsCache.caches.data[index];
            }

            return null;
        }

        public ComponentsCache<T> GetCache<T>() where T : struct, IComponent {
            var info = CacheTypeIdentifier<T>.info;
            if (this.typedCaches.TryGetValue(info.id, out var typedIndex)) {
                return ComponentsCache<T>.typedCaches.data[typedIndex];
            }

            ComponentsCache<T> componentsCache;
            if (info.isDisposable) {
                var constructedType = typeof(ComponentsCacheDisposable<>).MakeGenericType(typeof(T));
                componentsCache = (ComponentsCache<T>) Activator.CreateInstance(constructedType);
            }
            else {
                componentsCache = new ComponentsCache<T>();
            }

            this.caches.Add(info.id, componentsCache.commonCacheId, out _);
            this.typedCaches.Add(info.id, componentsCache.typedCacheId, out _);

            return componentsCache;
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

        public Entity CreateEntity() {
            var id = -1;
            if (this.freeEntityIDs.length > 0) {
                id = this.freeEntityIDs.Get(0);
                this.freeEntityIDs.RemoveAtSwap(0, out _);
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

        public Entity CreateEntity(out int id) {
            if (this.freeEntityIDs.length > 0) {
                id = this.freeEntityIDs.Get(0);
                this.freeEntityIDs.RemoveAtSwap(0, out _);
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
        internal Entity GetEntity(in int id) => this.entities[id];

        public void RemoveEntity(Entity entity) {
            var id = entity.ID;
            if (this.entities[id] == entity) {
                this.nextFreeEntityIDs.Add(id);
                this.entities[id] = null;
                --this.entitiesCount;
                entity.Dispose();
            }
        }

        public void UpdateFilters() {
            for (var index = 0; index < this.dirtyEntities.length; index++) {
                this.dirtyEntities.data[index].ApplyTransfer();
            }

            this.dirtyEntities.length = 0;

            if (this.newArchetypes.length > 0) {
                for (var index = 0; index < this.filters.length; index++) {
                    this.filters.data[index].FindArchetypes(this.newArchetypes);
                }

                this.newArchetypes.Clear();
            }

            for (var index = 0; index < this.archetypes.length; index++) {
                var archetype = this.archetypes.data[index];
                if (archetype.isDirty) {
                    archetype.Process();
                }
            }

            for (int index = 0, length = this.filters.length; index < length; index++) {
                var filter = this.filters.data[index];
                if (filter.isDirty) {
                    filter.UpdateLength();
                }
            }

            if (this.nextFreeEntityIDs.length > 0) {
                this.freeEntityIDs.AddListRange(this.nextFreeEntityIDs);
                this.nextFreeEntityIDs.Clear();
            }
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype : IDisposable {
        [SerializeField]
        internal int[] typeIds;
        [SerializeField]
        internal bool isDirty;
        [SerializeField]
        internal FastList<Entity> entities;
        [NonSerialized]
        internal FastList<Filter> filters;
        [NonSerialized]
        internal FastList<ComponentsBagPart> bagParts;
        [SerializeField]
        internal UnsafeIntHashMap<int> removeTransfer;
        [SerializeField]
        internal UnsafeIntHashMap<int> addTransfer;
        [SerializeField]
        internal int length;
        [SerializeField]
        internal int worldId;
        [SerializeField]
        internal int id;

        //todo support hotreload
        [NonSerialized]
        internal World world;

        internal Archetype(int id, int[] typeIds, int worldId) {
            this.id             = id;
            this.typeIds        = typeIds;
            this.length         = 0;
            this.entities       = new FastList<Entity>();
            this.addTransfer    = new UnsafeIntHashMap<int>();
            this.removeTransfer = new UnsafeIntHashMap<int>();
            this.bagParts       = new FastList<ComponentsBagPart>();

            this.isDirty = false;
            this.worldId = worldId;

            this.Ctor();
        }

        internal void Ctor() {
            this.world   = World.worlds.data[this.worldId];
            this.filters = new FastList<Filter>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Entity entity, out int index) {
            index = this.entities.length;
            this.entities.Add(entity);
            for (var i = 0; i < this.bagParts.length; i++) {
                this.bagParts.data[i].Add(entity);
            }

            this.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(Entity entity) {
            var index = entity.indexInCurrentArchetype;
            this.entities.RemoveAtSwap(index, out _);
            this.entities.data[index].indexInCurrentArchetype = index;
            for (var i = 0; i < this.bagParts.length; i++) {
                this.bagParts.data[i].Remove(index);
            }

            this.isDirty = true;
        }

        public void AddFilter(Filter filter) {
            this.filters.Add(filter);
            this.isDirty = true;
        }

        public void RemoveFilter(Filter filter) {
            this.filters.Remove(filter);
            this.isDirty = true;
        }

        public void Process() {
            for (int i = 0, len = this.filters.length; i < len; i++) {
                this.filters.data[i].isDirty = true;
            }

            this.length  = this.entities.length;
            this.isDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTransfer(int typeId, out int archetypeId, out Archetype archetype) {
            if (this.addTransfer.TryGetValue(typeId, out archetypeId)) {
                archetype = this.world.archetypes.data[archetypeId];
            }
            else {
                archetype = this.world.GetArchetype(this.typeIds, typeId, true, out archetypeId);
                this.addTransfer.Add(typeId, archetypeId, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveTransfer(int typeId, out int archetypeId, out Archetype archetype) {
            if (this.removeTransfer.TryGetValue(typeId, out archetypeId)) {
                archetype = this.world.archetypes.data[archetypeId];
            }
            else {
                archetype = this.world.GetArchetype(this.typeIds, typeId, false, out archetypeId);
                this.removeTransfer.Add(typeId, archetypeId, out _);
            }
        }

        internal ComponentsBagPart<T> Select<T>(int typeId) where T : struct, IComponent {
            for (int i = 0, len = this.bagParts.length; i < len; i++) {
                var bag = this.bagParts.data[i];
                if (bag.typeId == typeId) {
                    return (ComponentsBagPart<T>) bag;
                }
            }

            var bagPart = new ComponentsBagPart<T>(this);
            this.bagParts.Add(bagPart);

            return bagPart;
        }

        public void Dispose() {
            this.id      = -1;
            this.length  = -1;
            this.isDirty = false;

            this.typeIds = null;
            this.world   = null;

            this.entities.Clear();
            this.entities = null;

            this.addTransfer.Clear();
            this.addTransfer = null;

            this.removeTransfer.Clear();
            this.removeTransfer = null;
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        internal abstract class ComponentsBagPart {
            internal int typeId;

            internal IntFastList ids;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal abstract void Add(Entity entity);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal abstract void Remove(int index);
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        internal sealed class ComponentsBagPart<T> : ComponentsBagPart where T : struct, IComponent {
            internal World world;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ComponentsBagPart(Archetype archetype) {
                this.world = archetype.world;

                this.typeId = CacheTypeIdentifier<T>.info.id;
                this.ids    = new IntFastList(archetype.entities.length);

                foreach (var entity in archetype.entities) {
                    this.ids.Add(entity.GetComponentFast(this.typeId));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override void Add(Entity entity) => this.ids.Add(entity.componentsIds.GetValueByKey(this.typeId));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override void Remove(int index) => this.ids.RemoveAtSwap(index, out _);
        }
    }

    //TODO Separate RootFilter and ChildFilter
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class Filter : IEnumerable<Entity>, IDisposable {
        private enum FilterMode {
            None    = 0,
            Include = 1,
            Exclude = 2
        }

        public int Length;

        private World world;

        private FastList<Filter>        childs;
        private FastList<Archetype>     archetypes;
        private FastList<ComponentsBag> componentsBags;

        private IntFastList includedTypeIds;
        private IntFastList excludedTypeIds;

        private int        typeID;
        private FilterMode filterMode;

        internal bool isDirty;

        //root filter ctor
        //don't allocate any trash
        internal Filter(World world) {
            this.world = world;

            this.childs = new FastList<Filter>();

            this.typeID     = -1;
            this.filterMode = FilterMode.Include;
        }

        //full child filter
        private Filter(World world, int typeID, IntFastList includedTypeIds, IntFastList excludedTypeIds, FilterMode mode) {
            this.world = world;

            this.childs     = new FastList<Filter>();
            this.archetypes = new FastList<Archetype>();

            this.typeID          = typeID;
            this.includedTypeIds = includedTypeIds;
            this.excludedTypeIds = excludedTypeIds;

            this.filterMode = mode;

            this.componentsBags = new FastList<ComponentsBag>();

            this.world.filters.Add(this);

            this.FindArchetypes();

            this.UpdateLength();
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

            if (this.componentsBags != null) {
                foreach (var bag in this.componentsBags) {
                    bag.InternalDispose();
                }

                this.componentsBags.Clear();
            }

            this.componentsBags = null;

            this.typeID     = -1;
            this.filterMode = FilterMode.None;
        }

        [Obsolete("Use World.UpdateFilters()")]
        public void Update() {
            this.world.UpdateFilters();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UpdateLength() {
            this.isDirty = false;
            this.Length  = 0;
            foreach (var archetype in this.archetypes) {
                this.Length += archetype.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FindArchetypes(IntFastList newArchetypes) {
            var minLength = this.includedTypeIds.length;
            foreach (var archId in newArchetypes) {
                var arch = this.world.archetypes.data[archId];
                this.CheckArchetype(arch, minLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FindArchetypes() {
            var minLength = this.includedTypeIds.length;
            foreach (var arch in this.world.archetypes) {
                this.CheckArchetype(arch, minLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckArchetype(Archetype archetype, int minLength) {
            var typeIdsLength = archetype.typeIds.Length;
            if (typeIdsLength >= minLength) {
                var check = true;
                for (int i = 0, length = minLength; i < length; i++) {
                    var includedTypeId = this.includedTypeIds.Get(i);
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

                for (int i = 0, length = this.excludedTypeIds.length; i < length; i++) {
                    var excludedTypeId = this.excludedTypeIds.Get(i);
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
                if (check) {
                    for (int i = 0, length = this.archetypes.length; i < length; i++) {
                        if (this.archetypes.data[i] == archetype) {
                            return;
                        }
                    }

                    this.archetypes.Add(archetype);
                    archetype.AddFilter(this);
                    for (int i = 0, length = this.componentsBags.length; i < length; i++) {
                        var bag = this.componentsBags.data[i];
                        bag.AddArchetype(archetype);
                    }
                }
            }
        }

        public ComponentsBag<T> Select<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            if (typeInfo.isMarker) {
#if UNITY_EDITOR
                Debug.LogError($"You Select<{typeof(T)}> marker component from filter! This makes no sense.");
#endif
                return null;
            }

            for (int i = 0, length = this.componentsBags.length; i < length; i++) {
                var bag = this.componentsBags.data[i];
                if (bag.typeId == typeInfo.id) {
                    return (ComponentsBag<T>) bag;
                }
            }

            var componentsBag = new ComponentsBag<T>(this);
            this.componentsBags.Add(componentsBag);

            return componentsBag;
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetEntity(in int id) {
            if (this.archetypes.length == 1) {
                return this.archetypes.data[0].entities.data[id];
            }

            var num = 0;
            for (int i = 0, length = this.archetypes.length; i < length; i++) {
                var archetype = this.archetypes.data[i];
                var check     = num + archetype.length;
                if (id < check) {
                    return archetype.entities.data[id - num];
                }

                num = check;
            }

            return default;
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity First() {
            for (int i = 0, length = this.archetypes.length; i < length; i++) {
                var archetype = this.archetypes.data[i];
                if (archetype.length > 0) {
                    return archetype.entities.data[0];
                }
            }

            return default;
        }

        public Filter With<T>() where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Include);

        public Filter Without<T>() where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Exclude);

        private Filter CreateFilter<T>(FilterMode mode) where T : struct, IComponent {
            for (int i = 0, length = this.childs.length; i < length; i++) {
                var child = this.childs.data[i];
                if (child.filterMode == mode && child.typeID == CacheTypeIdentifier<T>.info.id) {
                    return child;
                }
            }

            var newTypeId = CacheTypeIdentifier<T>.info.id;

            IntFastList newIncludedTypeIds;
            IntFastList newExcludedTypeIds;
            if (this.typeID == -1) {
                newIncludedTypeIds = new IntFastList();
                newExcludedTypeIds = new IntFastList();
            }
            else {
                newIncludedTypeIds = new IntFastList(this.includedTypeIds);
                newExcludedTypeIds = new IntFastList(this.excludedTypeIds);
            }

            if (mode == FilterMode.Include) {
                newIncludedTypeIds.Add(newTypeId);
            }
            else if (mode == FilterMode.Exclude) {
                newExcludedTypeIds.Add(newTypeId);
            }

            var newFilter = new Filter(this.world, newTypeId, newIncludedTypeIds, newExcludedTypeIds, mode);
            this.childs.Add(newFilter);

            return newFilter;
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public abstract class ComponentsBag : IDisposable {
            public            int  typeId;
            internal abstract void AddArchetype(Archetype archetype);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal abstract void InternalDispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public abstract void Dispose();
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed class ComponentsBag<T> : ComponentsBag where T : struct, IComponent {
            private FastList<T> components;
            private Filter      filter;
            private IntFastList firstPartIds;

            private FastList<Archetype.ComponentsBagPart> parts;

            public ComponentsBag(Filter filter) {
                this.typeId = CacheTypeIdentifier<T>.info.id;

                this.parts      = new FastList<Archetype.ComponentsBagPart>();
                this.filter     = filter;
                this.components = filter.world.GetCache<T>().components;

                foreach (var archetype in filter.archetypes) {
                    this.parts.Add(archetype.Select<T>(this.typeId));
                }

                this.firstPartIds = this.parts.length > 0 ? this.parts.data[0].ids : null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T GetComponent(in int index) {
                if (this.parts.length > 1) {
                    int offset = 0;
                    for (int i = 0, length = this.parts.length; i < length; i++) {
                        var part  = this.parts.data[i];
                        var check = offset + part.ids.length;
                        if (index < check) {
                            return ref this.components.data[part.ids.Get(index - offset)];
                        }

                        offset = check;
                    }
                }

                return ref this.components.data[this.firstPartIds.Get(index)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetComponent(in int index, in T value) {
                if (this.parts.length > 1) {
                    int offset = 0;
                    for (int i = 0, length = this.parts.length; i < length; i++) {
                        var part  = this.parts.data[i];
                        var check = offset + part.ids.length;
                        if (index < check) {
                            this.components.data[part.ids.Get(index - offset)] = value;
                        }

                        offset = check;
                    }
                }
                else {
                    this.components.data[this.firstPartIds.Get(index)] = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override void AddArchetype(Archetype archetype) {
                var part = archetype.Select<T>(this.typeId);
                if (this.parts.length == 0) {
                    this.firstPartIds = part.ids;
                }

                this.parts.Add(part);
            }

            internal override void InternalDispose() {
                this.components = null;
                this.filter     = null;

                this.parts.Clear();
                this.parts = null;
            }

            public override void Dispose() {
                this.filter.componentsBags.Remove(this);
                this.InternalDispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityEnumerator GetEnumerator() {
            return new EntityEnumerator(this);
        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public struct EntityEnumerator : IEnumerator<Entity> {
            private readonly World               world;
            private readonly FastList<Archetype> archetypes;

            private int    index;
            private Entity current;

            private FastList<Entity> archetype;

            private int archetypeId;
            private int archetypeCount;

            internal EntityEnumerator(Filter filter) {
                this.world      = filter.world;
                this.archetypes = filter.archetypes;
                this.current    = null;
                this.index      = 0;

                this.archetypeId    = 0;
                this.archetypeCount = this.archetypes.length;
                this.archetype      = this.archetypeCount == 0 ? null : this.archetypes.data[0].entities;
            }

            public bool MoveNext() {
                if (this.archetypeCount == 1) {
                    if (this.index < this.archetype.length) {
                        this.current = this.archetype.data[this.index];
                        ++this.index;
                        return true;
                    }

                    return false;
                }

                if (this.archetypeId < this.archetypeCount) {
                    if (this.index < this.archetype.length) {
                        this.current = this.archetype.data[this.index];
                        ++this.index;
                        return true;
                    }

                    while (++this.archetypeId < this.archetypeCount) {
                        this.archetype = this.archetypes.data[this.archetypeId].entities;
                        if (this.archetype.length > 0) {
                            this.index   = 0;
                            this.current = this.archetype.data[this.index];
                            return true;
                        }
                    }
                }

                return false;
            }

            public void Reset() {
                this.index       = 0;
                this.current     = null;
                this.archetypeId = 0;
                this.archetype   = this.archetypeCount == 0 ? null : this.archetypes.data[0].entities;
            }

            public Entity Current => this.current;

            object IEnumerator.Current => this.current;

            public void Dispose() {
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
    public abstract class ComponentsCache : IDisposable {
        internal static FastList<ComponentsCache> caches = new FastList<ComponentsCache>();

        internal static Action cleanup = () => caches.Clear();

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
    public class ComponentsCache<T> : ComponentsCache where T : struct, IComponent {
        internal static FastList<ComponentsCache<T>> typedCaches = new FastList<ComponentsCache<T>>();

        [SerializeField]
        internal FastList<T> components;
        [SerializeField]
        internal IntStack freeIndexes;
        [SerializeField]
        internal int typedCacheId;
        [SerializeField]
        internal int typeId;

        static ComponentsCache() {
            cleanup += () => typedCaches.Clear();
        }

        internal ComponentsCache() {
            this.typeId = CacheTypeIdentifier<T>.info.id;

            this.components  = new FastList<T>(Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY);
            this.freeIndexes = new IntStack();

            this.components.Add();

            this.typedCacheId = typedCaches.length;
            typedCaches.Add(this);

            this.commonCacheId = caches.length;
            caches.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Add() {
            if (this.freeIndexes.length > 0) {
                return this.freeIndexes.Pop();
            }

            return this.components.Add();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Add(in T value) {
            if (this.freeIndexes.length > 0) {
                var index = this.freeIndexes.Pop();

                this.components.data[index] = value;
                return index;
            }

            return this.components.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent(in Entity entity) {
            var componentId = this.Add();
            if (entity.AddComponentFast(this.typeId, componentId)) {
                return ref this.components.data[componentId];
            }

            this.Remove(componentId);
            return ref this.components.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddComponent(in Entity entity, in T value) {
            var componentId = this.Add(value);
            if (entity.AddComponentFast(this.typeId, componentId)) {
                return true;
            }

            this.Remove(componentId);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Get(int id) => ref this.components.data[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent(in Entity entity) => ref this.components.data[entity.GetComponentFast(this.typeId)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Set(in int id, in T value) => this.components.data[id] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent(in Entity entity, in T value) => this.components.data[entity.GetComponentFast(this.typeId)] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Empty() => ref this.components.data[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Remove(in int id) {
            this.components.data[id] = default;
            this.freeIndexes.Push(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent(in Entity entity) {
            if (entity.RemoveComponentFast(this.typeId, out var cacheIndex)) {
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
    internal sealed class ComponentsCacheDisposable<T> : ComponentsCache<T> where T : struct, IComponent, IDisposable {
        internal override void Remove(in int id) {
            this.components.data[id].Dispose();
            base.Remove(in id);
        }

        public override void Dispose() {
            for (int i = 0, length = this.components.length; i < length; i++) {
                this.components.data[i].Dispose();
            }

            base.Dispose();
        }
    }

    namespace Utils {
        [Serializable]
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed unsafe class IntHashSet : IEnumerable<int> {
            public int length;
            public int capacity;
            public int capacityMinusOne;
            public int lastIndex;
            public int freeIndex;

            public int[] buckets;

            private int[] slots;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntHashSet() : this(0) {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntHashSet(int capacity) {
                this.lastIndex = 0;
                this.length    = 0;
                this.freeIndex = -1;

                this.capacityMinusOne = HashHelpers.GetPrime(capacity);
                this.capacity         = this.capacityMinusOne + 1;
                this.buckets          = new int[this.capacity];
                this.slots            = new int[this.capacity / 2];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Add(in int value) {
                var rem = value & this.capacityMinusOne;

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0]) {
                    int* slot;
                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                        slot = slotsPtr + i;
                        if (*slot - 1 == value) {
                            return false;
                        }
                    }
                }

                int newIndex;
                if (this.freeIndex >= 0) {
                    newIndex = this.freeIndex;
                    fixed (int* s = &this.slots[0]) {
                        this.freeIndex = *(s + newIndex + 1);
                    }
                }
                else {
                    if (this.lastIndex == this.capacity * 2) {
                        var newCapacityMinusOne = HashHelpers.ExpandPrime(this.length);
                        var newCapacity         = newCapacityMinusOne + 1;

                        ArrayHelpers.Grow(ref this.slots, newCapacity * 2);

                        var newBuckets = new int[newCapacity];

                        fixed (int* slotsPtr = &this.slots[0])
                        fixed (int* bucketsPtr = &newBuckets[0]) {
                            for (int i = 0, len = this.lastIndex; i < len; i += 2) {
                                var slotPtr = slotsPtr + i;

                                var newResizeIndex   = (*slotPtr - 1) & newCapacityMinusOne;
                                var newCurrentBucket = bucketsPtr + newResizeIndex;

                                *(slotPtr + 1) = *newCurrentBucket - 1;

                                *newCurrentBucket = i + 1;
                            }
                        }

                        this.buckets          = newBuckets;
                        this.capacityMinusOne = newCapacityMinusOne;
                        this.capacity         = newCapacity;

                        rem = value & newCapacityMinusOne;
                    }

                    newIndex       =  this.lastIndex;
                    this.lastIndex += 2;
                }

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0]) {
                    var bucket = bucketsPtr + rem;
                    var slot   = slotsPtr + newIndex;

                    *slot       = value + 1;
                    *(slot + 1) = *bucket - 1;

                    *bucket = newIndex + 1;
                }

                ++this.length;
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in int value) {
                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0]) {
                    var rem = value & this.capacityMinusOne;

                    int next;
                    var num = -1;

                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                        var slot     = slotsPtr + i;
                        var slotNext = slot + 1;

                        if (*slot - 1 == value) {
                            if (num < 0) {
                                *(bucketsPtr + rem) = *slotNext + 1;
                            }
                            else {
                                *(slotsPtr + num + 1) = *slotNext;
                            }

                            *slot     = -1;
                            *slotNext = this.freeIndex;

                            if (--this.length == 0) {
                                this.lastIndex = 0;
                                this.freeIndex = -1;
                            }
                            else {
                                this.freeIndex = i;
                            }

                            return true;
                        }

                        next = *slotNext;
                        num  = i;
                    }

                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int[] array) {
                fixed (int* slotsPtr = &this.slots[0]) {
                    var num = 0;
                    for (int i = 0, li = this.lastIndex, len = this.length; i < li && num < len; ++i) {
                        var v = *(slotsPtr + i) - 1;
                        if (v < 0) {
                            continue;
                        }

                        array[num] = v;
                        ++num;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Has(in int key) {
                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0]) {
                    var rem = key & this.capacityMinusOne;

                    int next;
                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                        var slot = slotsPtr + i;
                        if (*slot - 1 == key) {
                            return true;
                        }

                        next = *(slot + 1);
                    }

                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear() {
                if (this.lastIndex <= 0) {
                    return;
                }

                Array.Clear(this.slots, 0, this.lastIndex);
                Array.Clear(this.buckets, 0, this.capacityMinusOne);
                this.lastIndex = 0;
                this.length    = 0;
                this.freeIndex = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() {
                Enumerator e;
                e.set     = this;
                e.index   = 0;
                e.current = default;
                return e;
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Enumerator : IEnumerator<int> {
                public IntHashSet set;

                public int index;
                public int current;

                public bool MoveNext() {
                    fixed (int* slotsPtr = &this.set.slots[0]) {
                        for (var len = this.set.lastIndex; this.index < len; ++this.index) {
                            var v = *slotsPtr - 1;
                            if (v < 0) {
                                continue;
                            }

                            this.current = v;
                            ++this.index;

                            return true;
                        }

                        this.index   = this.set.lastIndex + 1;
                        this.current = default;
                        return false;
                    }
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
        public sealed class IntHashMap<T> : IEnumerable<int> {
            public int length;
            public int capacity;
            public int capacityMinusOne;
            public int lastIndex;
            public int freeIndex;

            public int[] buckets;

            private T[]    data;
            private Slot[] slots;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntHashMap(in int capacity = 0) {
                this.lastIndex = 0;
                this.length    = 0;
                this.freeIndex = -1;

                this.capacityMinusOne = HashHelpers.GetPrime(capacity);
                this.capacity         = this.capacityMinusOne + 1;

                this.buckets = new int[this.capacity];
                this.slots   = new Slot[this.capacity];
                this.data    = new T[this.capacity];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Add(in int key, in T value, out int slotIndex) {
                var rem = key & this.capacityMinusOne;

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key - 1 == key) {
                        slotIndex = -1;
                        return false;
                    }
                }

                if (this.freeIndex >= 0) {
                    slotIndex      = this.freeIndex;
                    this.freeIndex = this.slots[slotIndex].next;
                }
                else {
                    if (this.lastIndex == this.capacity) {
                        var newCapacityMinusOne = HashHelpers.ExpandPrime(this.length);
                        var newCapacity         = newCapacityMinusOne + 1;

                        ArrayHelpers.Grow(ref this.slots, newCapacity);
                        ArrayHelpers.Grow(ref this.data, newCapacity);

                        var newBuckets = new int[newCapacity];

                        for (int i = 0, len = this.lastIndex; i < len; ++i) {
                            ref var slot = ref this.slots[i];

                            var newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                            slot.next = newBuckets[newResizeIndex] - 1;

                            newBuckets[newResizeIndex] = i + 1;
                        }

                        this.buckets          = newBuckets;
                        this.capacity         = newCapacity;
                        this.capacityMinusOne = newCapacityMinusOne;

                        rem = key & this.capacityMinusOne;
                    }

                    slotIndex = this.lastIndex;
                    ++this.lastIndex;
                }

                ref var newSlot = ref this.slots[slotIndex];

                newSlot.key  = key + 1;
                newSlot.next = this.buckets[rem] - 1;

                this.data[slotIndex] = value;

                this.buckets[rem] = slotIndex + 1;

                ++this.length;
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(in int key, in T value, out int slotIndex) {
                var rem = key & this.capacityMinusOne;

                for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                    if (this.slots[i].key - 1 == key) {
                        this.data[i] = value;
                        slotIndex    = i;
                        return;
                    }
                }

                if (this.freeIndex >= 0) {
                    slotIndex      = this.freeIndex;
                    this.freeIndex = this.slots[slotIndex].next;
                }
                else {
                    if (this.lastIndex == this.capacity) {
                        var newCapacityMinusOne = HashHelpers.ExpandPrime(this.length);
                        var newCapacity         = newCapacityMinusOne + 1;

                        ArrayHelpers.Grow(ref this.slots, newCapacity);
                        ArrayHelpers.Grow(ref this.data, newCapacity);

                        var newBuckets = new int[newCapacity];

                        for (int i = 0, len = this.lastIndex; i < len; ++i) {
                            ref var slot           = ref this.slots[i];
                            var     newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                            slot.next = newBuckets[newResizeIndex] - 1;

                            newBuckets[newResizeIndex] = i + 1;
                        }

                        this.buckets          = newBuckets;
                        this.capacity         = newCapacity;
                        this.capacityMinusOne = newCapacityMinusOne;

                        rem = key & this.capacityMinusOne;
                    }

                    slotIndex = this.lastIndex;
                    ++this.lastIndex;
                }

                ref var newSlot = ref this.slots[slotIndex];

                newSlot.key  = key + 1;
                newSlot.next = this.buckets[rem] - 1;

                this.data[slotIndex] = value;

                this.buckets[rem] = slotIndex + 1;

                ++this.length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in int key, [CanBeNull] out T lastValue) {
                var rem = key & this.capacityMinusOne;

                int next;
                int num = -1;
                for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                    ref var slot = ref this.slots[i];
                    if (slot.key - 1 == key) {
                        if (num < 0) {
                            this.buckets[rem] = slot.next + 1;
                        }
                        else {
                            this.slots[num].next = slot.next;
                        }

                        lastValue = this.data[i];

                        slot.key  = -1;
                        slot.next = this.freeIndex;

                        --this.length;
                        if (this.length == 0) {
                            this.lastIndex = 0;
                            this.freeIndex = -1;
                        }
                        else {
                            this.freeIndex = i;
                        }

                        return true;
                    }

                    next = slot.next;
                    num  = i;
                }

                lastValue = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Has(in int key) {
                var rem = key & this.capacityMinusOne;

                int next;
                for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                    ref var slot = ref this.slots[i];
                    if (slot.key - 1 == key) {
                        return true;
                    }

                    next = slot.next;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in int key, [CanBeNull] out T value) {
                var rem = key & this.capacityMinusOne;

                int next;
                for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                    ref var slot = ref this.slots[i];
                    if (slot.key - 1 == key) {
                        value = this.data[i];
                        return true;
                    }

                    next = slot.next;
                }

                value = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T GetValueByKey(in int key) {
                var rem = key & this.capacityMinusOne;

                int next;
                for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                    ref var slot = ref this.slots[i];
                    if (slot.key - 1 == key) {
                        return this.data[i];
                    }

                    next = slot.next;
                }

                return default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T GetValueByIndex(in int index) => this.data[index];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetKeyByIndex(in int index) => this.slots[index].key;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TryGetIndex(in int key) {
                var rem = key & this.capacityMinusOne;

                int next;
                for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                    ref var slot = ref this.slots[i];
                    if (slot.key - 1 == key) {
                        return i;
                    }

                    next = slot.next;
                }

                return -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] array) {
                int num = 0;
                for (int i = 0, li = this.lastIndex; i < li && num < this.length; ++i) {
                    if (this.slots[i].key - 1 < 0) {
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
                Array.Clear(this.buckets, 0, this.capacity);
                Array.Clear(this.data, 0, this.capacity);

                this.lastIndex = 0;
                this.length    = 0;
                this.freeIndex = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() {
                Enumerator e;
                e.hashMap = this;
                e.index   = 0;
                e.current = default;
                return e;
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

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
                public IntHashMap<T> hashMap;

                public int index;
                public int current;

                public bool MoveNext() {
                    for (; this.index < this.hashMap.lastIndex; ++this.index) {
                        ref var slot = ref this.hashMap.slots[this.index];
                        if (slot.key - 1 < 0) {
                            continue;
                        }

                        this.current = this.index;
                        ++this.index;

                        return true;
                    }

                    this.index   = this.hashMap.lastIndex + 1;
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
        public sealed unsafe class UnsafeIntHashMap<T> : IEnumerable<int> where T : unmanaged {
            public int length;
            public int capacity;
            public int capacityMinusOne;
            public int lastIndex;
            public int freeIndex;

            public int[] buckets;

            private T[]   data;
            private int[] slots;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnsafeIntHashMap(in int capacity = 0) {
                this.lastIndex = 0;
                this.length    = 0;
                this.freeIndex = -1;

                this.capacityMinusOne = HashHelpers.GetPrime(capacity);
                this.capacity         = this.capacityMinusOne + 1;

                this.buckets = new int[this.capacity];
                this.slots   = new int[this.capacity * 2];
                this.data    = new T[this.capacity];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Add(in int key, in T value, out int slotIndex) {
                var rem = key & this.capacityMinusOne;

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0]) {
                    int* slot;
                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                        slot = slotsPtr + i;
                        if (*slot - 1 == key) {
                            slotIndex = -1;
                            return false;
                        }
                    }
                }

                if (this.freeIndex >= 0) {
                    slotIndex = this.freeIndex;
                    fixed (int* s = &this.slots[0]) {
                        this.freeIndex = *(s + slotIndex + 1);
                    }
                }
                else {
                    if (this.lastIndex == this.capacity * 2) {
                        var newCapacityMinusOne = HashHelpers.ExpandPrime(this.length);
                        var newCapacity         = newCapacityMinusOne + 1;

                        ArrayHelpers.Grow(ref this.slots, newCapacity * 2);
                        ArrayHelpers.Grow(ref this.data, newCapacity);

                        var newBuckets = new int[newCapacity];

                        fixed (int* slotsPtr = &this.slots[0])
                        fixed (int* bucketsPtr = &newBuckets[0]) {
                            for (int i = 0, len = this.lastIndex; i < len; i += 2) {
                                var slotPtr = slotsPtr + i;

                                var newResizeIndex   = (*slotPtr - 1) & newCapacityMinusOne;
                                var newCurrentBucket = bucketsPtr + newResizeIndex;

                                *(slotPtr + 1) = *newCurrentBucket - 1;

                                *newCurrentBucket = i + 1;
                            }
                        }

                        this.buckets          = newBuckets;
                        this.capacity         = newCapacity;
                        this.capacityMinusOne = newCapacityMinusOne;

                        rem = key & this.capacityMinusOne;
                    }

                    slotIndex      =  this.lastIndex;
                    this.lastIndex += 2;
                }

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0])
                fixed (T* dataPtr = &this.data[0]) {
                    var bucket = bucketsPtr + rem;
                    var slot   = slotsPtr + slotIndex;

                    *slot       = key + 1;
                    *(slot + 1) = *bucket - 1;

                    *(dataPtr + slotIndex / 2) = value;

                    *bucket = slotIndex + 1;
                }

                ++this.length;
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in int key, out T lastValue) {
                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0])
                fixed (T* dataPtr = &this.data[0]) {
                    var rem = key & this.capacityMinusOne;

                    int next;
                    var num = -1;

                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                        var slot     = slotsPtr + i;
                        var slotNext = slot + 1;

                        if (*slot - 1 == key) {
                            if (num < 0) {
                                *(bucketsPtr + rem) = *slotNext + 1;
                            }
                            else {
                                *(slotsPtr + num + 1) = *slotNext;
                            }

                            lastValue = *(dataPtr + i / 2);

                            *slot     = -1;
                            *slotNext = this.freeIndex;

                            if (--this.length == 0) {
                                this.lastIndex = 0;
                                this.freeIndex = -1;
                            }
                            else {
                                this.freeIndex = i;
                            }

                            return true;
                        }

                        next = *slotNext;
                        num  = i;
                    }

                    lastValue = default;
                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in int key, out T value) {
                var rem = key & this.capacityMinusOne;

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0])
                fixed (T* dataPtr = &this.data[0]) {
                    int* slot;
                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                        slot = slotsPtr + i;
                        if (*slot - 1 == key) {
                            value = *(dataPtr + i / 2);
                            return true;
                        }
                    }
                }

                value = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T GetValueByKey(in int key) {
                var rem = key & this.capacityMinusOne;

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0])
                fixed (T* dataPtr = &this.data[0]) {
                    int next;
                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                        if (*(slotsPtr + i) - 1 == key) {
                            return *(dataPtr + i / 2);
                        }

                        next = *(slotsPtr + i + 1);
                    }
                }

                return default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T GetValueByIndex(in int index) {
                fixed (T* d = &this.data[0]) {
                    return *(d + index / 2);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetKeyByIndex(in int index) {
                fixed (int* d = &this.slots[0]) {
                    return *(d + index);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TryGetIndex(in int key) {
                var rem = key & this.capacityMinusOne;

                fixed (int* slotsPtr = &this.slots[0])
                fixed (int* bucketsPtr = &this.buckets[0]) {
                    int* slot;
                    for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                        slot = slotsPtr + i;
                        if (*slot - 1 == key) {
                            return i;
                        }
                    }
                }

                return -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear() {
                if (this.lastIndex <= 0) {
                    return;
                }

                Array.Clear(this.slots, 0, this.lastIndex);
                Array.Clear(this.buckets, 0, this.capacity);
                Array.Clear(this.data, 0, this.capacity);

                this.lastIndex = 0;
                this.length    = 0;
                this.freeIndex = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() {
                Enumerator e;
                e.hashMap = this;
                e.index   = 0;
                e.current = default;
                return e;
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Enumerator : IEnumerator<int> {
                public UnsafeIntHashMap<T> hashMap;

                public int index;
                public int current;

                public bool MoveNext() {
                    fixed (int* slotsPtr = &this.hashMap.slots[0]) {
                        for (; this.index < this.hashMap.lastIndex; this.index += 2) {
                            if (*(slotsPtr + this.index) - 1 < 0) {
                                continue;
                            }

                            this.current = this.index;
                            ++this.index;

                            return true;
                        }
                    }

                    this.index   = this.hashMap.lastIndex + 1;
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
        public sealed unsafe class IntStack {
            public int length;
            public int capacity;

            private int[] data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntStack() {
                this.capacity = 4;
                this.data     = new int[this.capacity];
                this.length   = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(in int value) {
                if (this.length == this.capacity) {
                    ArrayHelpers.Grow(ref this.data, this.capacity <<= 1);
                }

                fixed (int* d = &this.data[0]) {
                    *(d + this.length++) = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Pop() {
                fixed (int* d = &this.data[0]) {
                    return *(d + this.length--);
                }
            }

            public void Clear() {
                this.data   = null;
                this.length = this.capacity = 0;
            }
        }

        [Serializable]
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed class FastList<T> : IEnumerable<T> {
            public T[] data;
            public int length;
            public int capacity;

            public EqualityComparer<T> comparer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FastList() {
                this.capacity = 3;
                this.data     = new T[this.capacity];
                this.length   = 0;

                this.comparer = EqualityComparer<T>.Default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FastList(int capacity) {
                this.capacity = HashHelpers.GetPrime(capacity);
                this.data     = new T[this.capacity];
                this.length   = 0;

                this.comparer = EqualityComparer<T>.Default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FastList(FastList<T> other) {
                this.capacity = other.capacity;
                this.data     = new T[this.capacity];
                this.length   = other.length;
                Array.Copy(other.data, 0, this.data, 0, this.length);

                this.comparer = other.comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Add() {
                var index = this.length;
                if (++this.length == this.capacity) {
                    ArrayHelpers.Grow(ref this.data, this.capacity <<= 1);
                }

                return index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Add(T value) {
                var index = this.length;
                if (++this.length == this.capacity) {
                    ArrayHelpers.Grow(ref this.data, this.capacity <<= 1);
                }

                this.data[index] = value;
                return index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddListRange(FastList<T> other) {
                if (other.length > 0) {
                    var newSize = this.length + other.length;
                    if (newSize > this.capacity) {
                        while (newSize > this.capacity) {
                            this.capacity <<= 1;
                        }

                        ArrayHelpers.Grow(ref this.data, this.capacity);
                    }

                    if (this == other) {
                        Array.Copy(this.data, 0, this.data, this.length, this.length);
                    }
                    else {
                        Array.Copy(other.data, 0, this.data, this.length, other.length);
                    }

                    this.length += other.length;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Swap(int source, int destination) => this.data[destination] = this.data[source];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(T value) => ArrayHelpers.IndexOf(this.data, value, this.comparer);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(T value) => this.RemoveAt(this.IndexOf(value));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveSwap(T value, out ResultSwap swap) => this.RemoveAtSwap(this.IndexOf(value), out swap);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveAt(int index) {
                --this.length;
                if (index < this.length) {
                    Array.Copy(this.data, index + 1, this.data, index, this.length - index);
                }

                this.data[this.length] = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool RemoveAtSwap(int index, out ResultSwap swap) {
                if (this.length-- > 1) {
                    swap.oldIndex = this.length;
                    swap.newIndex = index;

                    this.data[swap.newIndex] = this.data[swap.oldIndex];
                    return true;
                }

                swap = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear() {
                if (this.length <= 0) {
                    return;
                }

                Array.Clear(this.data, 0, this.length);
                this.length = 0;
            }

            //todo rework
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Sort() => Array.Sort(this.data, 0, this.length, null);

            //todo rework
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Sort(int index, int len) => Array.Sort(this.data, index, len, null);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T[] ToArray() {
                var newArray = new T[this.length];
                Array.Copy(this.data, 0, newArray, 0, this.length);
                return newArray;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() {
                Enumerator e;
                e.list    = this;
                e.current = default;
                e.index   = 0;
                return e;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct ResultSwap {
                public int oldIndex;
                public int newIndex;
            }

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Enumerator : IEnumerator<T> {
                public FastList<T> list;

                public T   current;
                public int index;

                public bool MoveNext() {
                    if (this.index >= this.list.length) {
                        return false;
                    }

                    this.current = this.list.data[this.index++];
                    return true;
                }

                public void Reset() {
                    this.index   = 0;
                    this.current = default;
                }

                public T           Current => this.current;
                object IEnumerator.Current => this.current;

                public void Dispose() {
                }
            }
        }

        [Serializable]
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public sealed unsafe class IntFastList : IEnumerable<int> {
            public int length;
            public int capacity;

            private int[] data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntFastList() {
                this.capacity = 3;
                this.data     = new int[this.capacity];
                this.length   = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntFastList(int capacity) {
                this.capacity = HashHelpers.GetPrime(capacity);
                this.data     = new int[this.capacity];
                this.length   = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IntFastList(IntFastList other) {
                this.capacity = other.capacity;
                this.data     = new int[this.capacity];
                this.length   = other.length;
                Array.Copy(other.data, 0, this.data, 0, this.length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Add() {
                var index = this.length;
                if (++this.length == this.capacity) {
                    ArrayHelpers.Grow(ref this.data, this.capacity <<= 1);
                }

                return index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Get(in int index) {
                fixed (int* d = &this.data[0]) {
                    return *(d + index);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(in int index, in int value) {
                fixed (int* d = &this.data[0]) {
                    *(d + index) = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Add(in int value) {
                var index = this.length;
                if (++this.length == this.capacity) {
                    ArrayHelpers.Grow(ref this.data, this.capacity <<= 1);
                }

                fixed (int* p = &this.data[0]) {
                    *(p + index) = value;
                }

                return index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddListRange(IntFastList other) {
                if (other.length > 0) {
                    var newSize = this.length + other.length;
                    if (newSize > this.capacity) {
                        while (newSize > this.capacity) {
                            this.capacity <<= 1;
                        }

                        ArrayHelpers.Grow(ref this.data, this.capacity);
                    }

                    if (this == other) {
                        Array.Copy(this.data, 0, this.data, this.length, this.length);
                    }
                    else {
                        Array.Copy(other.data, 0, this.data, this.length, other.length);
                    }

                    this.length += other.length;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Swap(int source, int destination) => this.data[destination] = this.data[source];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(int value) => ArrayHelpers.IndexOfUnsafeInt(this.data, value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(int value) => this.RemoveAt(this.IndexOf(value));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveSwap(int value, out ResultSwap swap) => this.RemoveAtSwap(this.IndexOf(value), out swap);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveAt(int index) {
                --this.length;
                if (index < this.length) {
                    Array.Copy(this.data, index + 1, this.data, index, this.length - index);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool RemoveAtSwap(int index, out ResultSwap swap) {
                if (this.length-- > 1) {
                    swap.oldIndex = this.length;
                    swap.newIndex = index;
                    fixed (int* d = &this.data[0]) {
                        *(d + swap.newIndex) = *(d + swap.oldIndex);
                    }

                    return true;
                }

                swap = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear() {
                if (this.length <= 0) {
                    return;
                }

                Array.Clear(this.data, 0, this.length);
                this.length = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Sort() => Array.Sort(this.data, 0, this.length, null);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Sort(int index, int len) => Array.Sort(this.data, index, len, null);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int[] ToArray() {
                var newArray = new int[this.length];
                Array.Copy(this.data, 0, newArray, 0, this.length);
                return newArray;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() {
                Enumerator e;
                e.intFastList = this;
                e.current     = default;
                e.index       = 0;
                return e;
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct ResultSwap {
                public int oldIndex;
                public int newIndex;
            }

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            public struct Enumerator : IEnumerator<int> {
                public IntFastList intFastList;

                public int current;
                public int index;

                public bool MoveNext() {
                    if (this.index >= this.intFastList.length) {
                        return false;
                    }

                    fixed (int* d = &this.intFastList.data[0]) {
                        this.current = *(d + this.index++);
                    }

                    return true;
                }

                public void Reset() {
                    this.index   = 0;
                    this.current = default;
                }

                public int         Current => this.current;
                object IEnumerator.Current => this.current;

                public void Dispose() {
                }
            }
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        internal static class ArrayHelpers {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Grow<T>(ref T[] array, int newSize) {
                var newArray = new T[newSize];
                Array.Copy(array, 0, newArray, 0, array.Length);
                array = newArray;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int IndexOf<T>(T[] array, T value, EqualityComparer<T> comparer) {
                for (int i = 0, length = array.Length; i < length; ++i) {
                    if (comparer.Equals(array[i], value)) {
                        return i;
                    }
                }

                return -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe int IndexOfUnsafeInt(int[] array, int value) {
                fixed (int* arr = &array[0]) {
                    var i = 0;
                    for (int* current = arr, length = arr + array.Length; current < length; ++current) {
                        if (*current == value) {
                            return i;
                        }

                        ++i;
                    }
                }


                return -1;
            }
        }

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        internal static class HashHelpers {
            //https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Collections/HashHelpers.cs#L32
            //different primes to fit n^2 - 1
            internal static readonly int[] primes = {
                3,
                15,
                63,
                255,
                1023,
                4095,
                16383,
                65535,
                262143,
                1048575,
                4194303,
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
                    if (IsPrime(candidate) && (candidate - 1) % 101 != 0)
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