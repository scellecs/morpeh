[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests.Runtime")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests.Editor")]

namespace Morpeh {
#if UNITY_2019_3
    using UnityEngine.LowLevel;
    using UnityEngine.PlayerLoop;
#else
    using UnityEngine.Experimental.PlayerLoop;
    using UnityEngine.Experimental.LowLevel;
#endif
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Utils;
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;
    using Object = UnityEngine.Object;

    internal static class Constants {
        internal const int DEFAULT_WORLD_ENTITIES_CAPACITY             = 65536;
        internal const int DEFAULT_ENTITY_COMPONENTS_CAPACITY          = 2;
        internal const int DEFAULT_ROOT_FILTER_DIRTY_ENTITIES_CAPACITY = DEFAULT_WORLD_ENTITIES_CAPACITY;
        internal const int DEFAULT_FILTER_DIRTY_ENTITIES_CAPACITY      = 1024;
        internal const int DEFAULT_FILTER_ADDED_ENTITIES_CAPACITY      = 32;
        internal const int DEFAULT_FILTER_REMOVED_ENTITIES_CAPACITY    = 32;
        internal const int DEFAULT_CACHE_COMPONENTS_CAPACITY           = 2048;
    }

    public interface IEntity : IDisposable {
        int   ID { get; }
        ref T AddComponent<T>() where T : struct, IComponent;
        ref T AddComponent<T>(out bool exist) where T : struct, IComponent;

        ref T GetComponent<T>() where T : struct, IComponent;
        ref T GetComponent<T>(out bool exist) where T : struct, IComponent;

        void SetComponent<T>(in T value) where T : struct, IComponent;
        bool RemoveComponent<T>() where T : struct, IComponent;
        bool Has<T>() where T : struct, IComponent;
        bool Has(in FastBitMask mask);
    }

    public interface IComponent {
    }

    public interface IInitializer : IDisposable {
        World          World  { get; set; }
        FilterProvider Filter { get; set; }

        /// <summary>
        ///     Calling 1 time on registration in World
        /// </summary>
        void OnAwake();

        /// <summary>
        ///     Calling before update first system in frame, but after filters update
        /// </summary>
        void OnStart();
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
        internal int InternalID;

        internal FastBitMask ComponentsMask;
        internal World       World;

        private int[] components;
        private int   componentsDoubleCount;

#if UNITY_EDITOR
        [ShowInInspector]
#endif
        public int ID => this.InternalID;

        internal Entity(int id) {
            this.InternalID = id;

            this.componentsDoubleCount = 0;

            this.components     = new int[Constants.DEFAULT_ENTITY_COMPONENTS_CAPACITY];
            this.ComponentsMask = FastBitMask.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("You add component which already exist! Use Get or SetComponent instead!");
#endif
                return ref CacheComponents<T>.Empty();
            }

            this.ComponentsMask.SetBit(typeInfo.id);
            this.World.Filter.EntityChanged(this.InternalID);

            if (!typeInfo.isMarker) {
                var componentId = CacheComponents<T>.Add();
                for (int i = 0, length = this.componentsDoubleCount; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        this.components[i + 1] = componentId;
                        return ref CacheComponents<T>.Get(this.components[i + 1]);
                    }
                }

                this.componentsDoubleCount += 2;
                if (this.componentsDoubleCount >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCount << 1);
                }

                this.components[this.componentsDoubleCount - 2] = typeInfo.id;
                this.components[this.componentsDoubleCount - 1] = componentId;

                return ref CacheComponents<T>.Get(this.components[this.componentsDoubleCount - 1]);
            }

            return ref CacheComponents<T>.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("You add component which already exist! Use Get or SetComponent instead!");
#endif
                exist = true;
                return ref CacheComponents<T>.Empty();
            }

            exist = false;
            this.ComponentsMask.SetBit(typeInfo.id);
            this.World.Filter.EntityChanged(this.InternalID);

            if (!typeInfo.isMarker) {
                var componentId = CacheComponents<T>.Add();
                for (int i = 0, length = this.componentsDoubleCount; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        this.components[i + 1] = componentId;
                        return ref CacheComponents<T>.Get(this.components[i + 1]);
                    }
                }

                this.componentsDoubleCount += 2;
                if (this.componentsDoubleCount >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCount << 1);
                }

                this.components[this.componentsDoubleCount - 2] = typeInfo.id;
                this.components[this.componentsDoubleCount - 1] = componentId;

                return ref CacheComponents<T>.Get(this.components[this.componentsDoubleCount - 1]);
            }

            return ref CacheComponents<T>.Empty();
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetComponentId(int typeId) {
            var typeInfo = CommonCacheTypeIdentifier.editorTypeAssociation[typeId].Info;
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

            if (typeInfo.isMarker) {
                if (this.Has<T>()) {
                    return ref CacheComponents<T>.Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    return ref CacheComponents<T>.Get(this.components[i + 1]);
                }
            }

            return ref CacheComponents<T>.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (typeInfo.isMarker) {
                if (this.Has<T>()) {
                    exist = true;
                    return ref CacheComponents<T>.Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    exist = true;
                    return ref CacheComponents<T>.Get(this.components[i + 1]);
                }
            }

            exist = false;
            return ref CacheComponents<T>.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (!typeInfo.isMarker) {
                if (!this.Has(typeInfo.mask)) {
                    var componentId = CacheComponents<T>.Add();
                    for (int i = 0, length = this.componentsDoubleCount; i < length; i += 2) {
                        if (this.components[i] == typeInfo.id) {
                            this.components[i + 1] = componentId;
                        }
                    }

                    this.componentsDoubleCount += 2;
                    if (this.componentsDoubleCount >= this.components.Length) {
                        Array.Resize(ref this.components, this.componentsDoubleCount << 1);
                    }

                    this.components[this.componentsDoubleCount - 2] = typeInfo.id;
                    this.components[this.componentsDoubleCount - 1] = componentId;
                    this.World.Filter.EntityChanged(this.InternalID);
                    CacheComponents<T>.Set(componentId, value);
                    this.ComponentsMask.SetBit(typeInfo.id);
                    return;
                }

                for (int i = 0, length = this.components.Length; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        CacheComponents<T>.Set(this.components[i + 1], value);
                    }
                }
            }
            else {
                if (!this.Has(typeInfo.mask)) {
                    this.World.Filter.EntityChanged(this.InternalID);
                }
            }

            this.ComponentsMask.SetBit(typeInfo.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;
            if (!typeInfo.isMarker) {
                for (int i = 0, length = this.components.Length; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        CacheComponents<T>.Remove(this.components[i + 1]);

                        this.ComponentsMask.ClearBit(typeInfo.id);
                        if (this.ComponentsMask == FastBitMask.None) {
                            this.World.RemoveEntity(this);
                        }
                        else {
                            this.World.Filter.EntityChanged(this.InternalID);
                        }

                        return true;
                    }
                }
            }
            else {
                if (this.ComponentsMask.Has(typeInfo.mask)) {
                    this.ComponentsMask.ClearBit(typeInfo.id);
                    if (this.ComponentsMask == FastBitMask.None) {
                        this.World.RemoveEntity(this);
                    }
                    else {
                        this.World.Filter.EntityChanged(this.InternalID);
                    }

                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(in FastBitMask mask) => this.ComponentsMask.Has(mask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has<T>() where T : struct, IComponent => this.ComponentsMask.Has(CacheTypeIdentifier<T>.info.mask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDisposed() => this.ComponentsMask == FastBitMask.None;

        public void Dispose() {
            var mask = new FastBitMask();
            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                var typeId = this.components[i];
                mask.SetBit(typeId);
                if (this.ComponentsMask.Has(mask)) {
                    ComponentsCleaner.Clean(typeId, this.components[i + 1]);
                }

                mask.ClearBit(typeId);
            }

            this.ComponentsMask = FastBitMask.None;
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class World : IDisposable {
        public static World Default { get; private set; }

        private static readonly List<World> Worlds = new List<World>();

        internal Filter Filter;

        private SortedList<int, ISystem> systems;
        private SortedList<int, ISystem> fixedSystems;
        private SortedList<int, ISystem> lateSystems;

        private SortedList<int, ISystem> disabledSystems;
        private SortedList<int, ISystem> disabledFixedSystems;
        private SortedList<int, ISystem> disabledLateSystems;

        private List<IInitializer> newInitializers;
        private List<IDisposable>  disposables;

        internal Entity[] Entities;

        internal int EntitiesCount;
        internal int EntitiesLength;
        internal int EntitiesCapacity;

        private Queue<int> freeEntityIDs;

        public World() {
            this.systems      = new SortedList<int, ISystem>();
            this.fixedSystems = new SortedList<int, ISystem>();
            this.lateSystems  = new SortedList<int, ISystem>();

            this.disabledSystems      = new SortedList<int, ISystem>();
            this.disabledFixedSystems = new SortedList<int, ISystem>();
            this.disabledLateSystems  = new SortedList<int, ISystem>();

            this.newInitializers = new List<IInitializer>();
            this.disposables     = new List<IDisposable>();

            this.EntitiesLength   = 0;
            this.EntitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            this.Entities         = new Entity[this.EntitiesCapacity];
            this.freeEntityIDs    = new Queue<int>();

            this.Filter = new Filter(this);
        }

        public void Dispose() {
            void DisposeSystems(SortedList<int, ISystem> systemsToDispose) {
                foreach (var system in systemsToDispose.Values) {
                    system.Dispose();
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

            foreach (var initializer in this.newInitializers) {
                initializer.Dispose();
            }

            this.newInitializers.Clear();
            this.newInitializers = null;

            foreach (var disposable in this.disposables) {
                disposable.Dispose();
            }

            this.disposables.Clear();
            this.disposables = null;

            foreach (var entity in this.Entities) {
                entity?.Dispose();
            }

            this.Entities         = null;
            this.EntitiesLength   = -1;
            this.EntitiesCapacity = -1;

            this.freeEntityIDs.Clear();
            this.freeEntityIDs = null;

            this.Filter.Dispose();
            this.Filter = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InitializationWithPlayerLoop() {
            Default = new World();
            Default.RegisterInDefaultPlayerLoop();

            var defaultPlayerLoop = PlayerLoop.GetDefaultPlayerLoop();
            for (int i = 0, length = defaultPlayerLoop.subSystemList.Length; i < length; i++) {
                var subSystem = defaultPlayerLoop.subSystemList[i];

                if (subSystem.type == typeof(PreLateUpdate)) {
                    defaultPlayerLoop.subSystemList[i].updateDelegate += PlayerLoopUpdate;
                }
                else if (subSystem.type == typeof(FixedUpdate)) {
                    var go = new GameObject {
                        name      = "MORPEH_FIXED_UPDATE_WORKAROUND",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    go.AddComponent<FixedUpdateWorkaround>();
                    Object.DontDestroyOnLoad(go);
                }
                else if (subSystem.type == typeof(PostLateUpdate)) {
                    defaultPlayerLoop.subSystemList[i].updateDelegate += PlayerLoopLateUpdate;
                }
            }

            PlayerLoop.SetPlayerLoop(defaultPlayerLoop);
        }

        private class FixedUpdateWorkaround : MonoBehaviour {
            private void FixedUpdate() => PlayerLoopFixedUpdate();
        }

        private static void PlayerLoopUpdate() {
            foreach (var world in Worlds) {
                world.Update();
            }
        }

        private static void PlayerLoopFixedUpdate() {
            foreach (var world in Worlds) {
                world.FixedUpdate();
            }
        }

        private static void PlayerLoopLateUpdate() {
            foreach (var world in Worlds) {
                world.LateUpdate();
            }
        }

        public void RegisterInDefaultPlayerLoop() {
            Worlds.Add(this);
        }

        public void UnregisterInDefaultPlayerLoop() {
            Worlds.Remove(this);
        }

        public void Update() {
            foreach (var disposable in this.disposables) {
                disposable.Dispose();
            }

            this.disposables.Clear();

            this.Filter.Update();

            foreach (var initializer in this.newInitializers) {
                initializer.OnStart();
                this.Filter.Update();
            }

            this.newInitializers.Clear();

            var dt = Time.deltaTime;
            for (int i = 0, length = this.systems.Count; i < length; i++) {
                this.systems.Values[i].OnUpdate(dt);
                this.Filter.Update();
            }
        }

        public void FixedUpdate() {
            this.Filter.Update();

            var dt = Time.fixedDeltaTime;
            for (int i = 0, length = this.fixedSystems.Count; i < length; i++) {
                this.fixedSystems.Values[i].OnUpdate(dt);
                this.Filter.Update();
            }
        }

        public void LateUpdate() {
            this.Filter.Update();

            var dt = Time.deltaTime;
            for (int i = 0, length = this.lateSystems.Count; i < length; i++) {
                this.lateSystems.Values[i].OnUpdate(dt);
                this.Filter.Update();
            }
        }

        public void AddInitializer<T>(T initializer) where T : class, IInitializer {
            initializer.World  = this;
            initializer.Filter = new FilterProvider {World = this};

            initializer.OnAwake();
            this.newInitializers.Add(initializer);
        }

        public void RemoveInitializer<T>(T initializer) where T : class, IInitializer
            => this.newInitializers.Remove(initializer);

        //TODO refactor for Order of Systems
        public bool AddSystem<T>(int order, T system) where T : class, ISystem {
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

            if (!collection.ContainsValue(system) && !disabledCollection.ContainsValue(system)) {
                collection.Add(order, system);
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

            if (disabledCollection.ContainsValue(system)) {
                var order = disabledCollection.Keys[disabledCollection.IndexOfValue(system)];
                collection.Add(order, system);
                disabledCollection.Remove(order);
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

            if (collection.ContainsValue(system)) {
                var order = collection.Keys[collection.IndexOfValue(system)];
                disabledCollection.Add(order, system);
                collection.Remove(order);
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

            if (collection.ContainsValue(system)) {
                var order   = collection.Keys[collection.IndexOfValue(system)];
                var deleted = collection.Remove(order);
                if (deleted) {
                    this.disposables.Add(system);
                    this.RemoveInitializer(system);
                    return true;
                }
            }
            else if (disabledCollection.ContainsValue(system)) {
                var order   = disabledCollection.Keys[disabledCollection.IndexOfValue(system)];
                var deleted = disabledCollection.Remove(order);
                if (deleted) {
                    this.disposables.Add(system);
                    this.RemoveInitializer(system);
                    return true;
                }
            }

            return false;
        }

        public IEntity CreateEntity() => this.CreateEntityInternal();

        internal Entity CreateEntityInternal() {
            var id = -1;
            if (this.freeEntityIDs.Count > 0) {
                id = this.freeEntityIDs.Dequeue();
            }
            else {
                id = this.EntitiesLength++;
            }

            if (this.EntitiesLength >= this.EntitiesCapacity) {
                var newCapacity = this.EntitiesCapacity << 1;
                Array.Resize(ref this.Entities, newCapacity);
                this.EntitiesCapacity = newCapacity;
            }

            this.Entities[id] = new Entity(id) {World = this};
            this.Filter.Entities.Add(id);
            ++this.EntitiesCount;

            return this.Entities[id];
        }

        public IEntity CreateEntity(out int id) => this.CreateEntityInternal(out id);

        internal Entity CreateEntityInternal(out int id) {
            if (this.freeEntityIDs.Count > 0) {
                id = this.freeEntityIDs.Dequeue();
            }
            else {
                id = this.EntitiesLength++;
            }

            if (this.EntitiesLength >= this.EntitiesCapacity) {
                var newCapacity = this.EntitiesCapacity << 1;
                Array.Resize(ref this.Entities, newCapacity);
                this.EntitiesCapacity = newCapacity;
            }

            this.Entities[id] = new Entity(id) {World = this};
            this.Filter.Entities.Add(id);
            ++this.EntitiesCount;

            return this.Entities[id];
        }

        public IEntity GetEntity(in int id) => this.Entities[id];

        public void RemoveEntity(IEntity entity) {
            if (entity is Entity ent) {
                var id = ent.ID;
                if (this.Entities[id] == ent) {
                    this.freeEntityIDs.Enqueue(id);
                    this.Filter.Entities.Remove(id);
                    this.Entities[id] = null;
                    --this.EntitiesCount;
                }
            }
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class FilterProvider : IDisposable {
        public Filter All => this.World.Filter;

        internal World World;

        public void Dispose() => this.World = null;
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

        public int Length;

        internal ObservableHashSet<int> Entities;

        private World world;

        private int[] entitiesCacheForBags;
        private int   entitiesCacheForBagsCapacity;

        // 0 - typeIndex
        // 1 - componentsBagCacheIndex
        // 2 - isDirty
        private int[] componentsBags;
        private int   componentsBagsTripleCount;

        private List<Filter> childs;
        private FastBitMask  mask;
        private FilterMode   filterMode;

        [CanBeNull]
        private List<int> addedList;
        [CanBeNull]
        private List<int> removedList;
        private List<int> dirtyList;


        //root filter ctor
        //don't allocate any trash
        internal Filter(World world) {
            this.world = world;

            this.Entities  = new ObservableHashSet<int>();
            this.dirtyList = new List<int>(Constants.DEFAULT_ROOT_FILTER_DIRTY_ENTITIES_CAPACITY);
            this.childs    = new List<Filter>();

            this.mask       = FastBitMask.None;
            this.filterMode = FilterMode.Include;

            this.entitiesCacheForBags         = new int[0];
            this.entitiesCacheForBagsCapacity = 0;
        }


        //full child filter
        private Filter(World world, ObservableHashSet<int> rootEntities, FastBitMask mask, FilterMode mode, bool fillWithPreviousEntities) {
            this.world = world;

            this.addedList   = new List<int>(Constants.DEFAULT_FILTER_ADDED_ENTITIES_CAPACITY);
            this.removedList = new List<int>(Constants.DEFAULT_FILTER_REMOVED_ENTITIES_CAPACITY);
            this.Entities    = new ObservableHashSet<int>();
            this.dirtyList   = new List<int>(Constants.DEFAULT_FILTER_DIRTY_ENTITIES_CAPACITY);
            this.childs      = new List<Filter>();

            this.mask       = mask;
            this.filterMode = mode;

            this.componentsBags       = new int[0];
            this.entitiesCacheForBags = new int[0];

            this.componentsBagsTripleCount    = 0;
            this.entitiesCacheForBagsCapacity = 0;

            rootEntities.OnAddItem    += item => this.addedList.Add(item);
            rootEntities.OnRemoveItem += item => this.removedList.Add(item);

            if (!fillWithPreviousEntities) {
                return;
            }

            foreach (var id in rootEntities) {
                var entity = this.world.GetEntity(id);
                switch (this.filterMode) {
                    case FilterMode.Include:
                        if (entity.Has(this.mask)) {
                            this.Entities.Add(id);
                        }

                        break;
                    case FilterMode.Exclude:
                        if (!entity.Has(this.mask)) {
                            this.Entities.Add(id);
                        }

                        break;
                }
            }

            this.Length = this.Entities.Count;
            this.CacheEntities();
        }

        public void Dispose() {
            foreach (var child in this.childs) {
                child.Dispose();
            }

            this.childs.Clear();
            this.childs = null;

            this.Length = -1;

            this.Entities.Clear();
            this.Entities = null;

            this.world = null;

            this.entitiesCacheForBags         = null;
            this.entitiesCacheForBagsCapacity = -1;

            this.componentsBags            = null;
            this.componentsBagsTripleCount = -1;

            this.mask       = FastBitMask.None;
            this.filterMode = FilterMode.None;

            this.addedList?.Clear();
            this.addedList = null;
            this.removedList?.Clear();
            this.removedList = null;

            this.dirtyList.Clear();
            this.dirtyList = null;
        }

        internal void EntityChanged(int id) => this.dirtyList.Add(id);

        public void Update() {
            if (this.mask == FastBitMask.None) {
                for (int i = 0, length = this.childs.Count; i < length; i++) {
                    this.childs[i].ChildrensUpdate(this.dirtyList);
                }

                this.dirtyList.Clear();

                return;
            }

            var changed = false;

            var originDirtyCount = this.dirtyList.Count;

            for (var i = this.dirtyList.Count - 1; i >= 0; i--) {
                var dirtyId = this.dirtyList[i];
                var entity  = this.world.GetEntity(dirtyId);
                //TODO maybe remove?
                if (entity != null) {
                    if (this.filterMode == FilterMode.Include) {
                        if (entity.Has(this.mask)) {
                            if (this.Entities.Add(dirtyId)) {
                                this.dirtyList.RemoveAtFast(i);
                            }
                        }
                        else {
                            this.Entities.Remove(dirtyId);
                            this.dirtyList.RemoveAtFast(i);
                        }
                    }

                    if (this.filterMode == FilterMode.Exclude) {
                        if (!entity.Has(this.mask)) {
                            if (this.Entities.Add(dirtyId)) {
                                this.dirtyList.RemoveAtFast(i);
                            }
                        }
                        else {
                            this.Entities.Remove(dirtyId);
                            this.dirtyList.RemoveAtFast(i);
                        }
                    }
                }
            }

            if (this.addedList.Count > 0 || this.removedList.Count > 0 || originDirtyCount != this.dirtyList.Count) {
                for (int i = 2, length = this.componentsBagsTripleCount; i < length; i += 3) {
                    this.componentsBags[i] = 1;
                }

                changed = true;
            }

            for (int i = 0, length = this.addedList.Count; i < length; i++) {
                var id     = this.addedList[i];
                var entity = this.world.GetEntity(id);
                switch (this.filterMode) {
                    case FilterMode.Include:
                        if (entity.Has(this.mask)) {
                            this.Entities.Add(id);
                        }

                        break;
                    case FilterMode.Exclude:
                        if (!entity.Has(this.mask)) {
                            this.Entities.Add(id);
                        }

                        break;
                }
            }

            this.addedList.Clear();

            for (int i = 0, length = this.removedList.Count; i < length; i++) {
                var id = this.removedList[i];
                this.Entities.Remove(id);
            }

            this.removedList.Clear();

            for (int i = 0, length = this.childs.Count; i < length; i++) {
                this.childs[i].ChildrensUpdate(this.dirtyList);
            }

            this.dirtyList.Clear();

            this.Length = this.Entities.Count;

            if (changed) {
                this.CacheEntities();
            }
        }

        private void CacheEntities() {
            if (this.entitiesCacheForBagsCapacity < this.Length) {
                Array.Resize(ref this.entitiesCacheForBags, this.Length);
                this.entitiesCacheForBagsCapacity = this.Length;
            }

            var i = 0;
            foreach (var id in this.Entities) {
                this.entitiesCacheForBags[i++] = id;
            }
        }

        private void ChildrensUpdate(List<int> parentDirtyList) {
            this.dirtyList.AddRange(parentDirtyList);
            this.Update();
        }

        public ref ComponentsBag<T> Select<T>() where T : struct, IComponent {
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
        public IEntity GetEntity(in int id) => this.world.Entities[this.entitiesCacheForBags[id]];

        public Filter With<T>(bool fillWithPreviousEntities = true) where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Include, fillWithPreviousEntities);

        public Filter Without<T>(bool fillWithPreviousEntities = true) where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Exclude, fillWithPreviousEntities);

        private Filter CreateFilter<T>(FilterMode mode, bool fillWithPreviousEntities) where T : struct, IComponent {
            for (int i = 0, length = this.childs.Count; i < length; i++) {
                var child = this.childs[i];
                if (child.filterMode == mode && child.mask == CacheTypeIdentifier<T>.info.mask) {
                    return child;
                }
            }

            var newFilter = new Filter(this.world, this.Entities, CacheTypeIdentifier<T>.info.mask, mode, fillWithPreviousEntities);
            this.childs.Add(newFilter);

            return newFilter;
        }

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

            //TODO possible slow resize
            internal void Update(int[] entities, in int len) {
                Array.Resize(ref this.ids, len);
                for (var i = 0; i < len; i++) {
                    this.ids[i] = this.world.Entities[entities[i]].GetComponentId<T>();
                }
            }

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T GetComponent(in int index) => ref this.sharedComponents[this.ids[index]];

            [Il2Cpp(Option.NullChecks, false)]
            [Il2Cpp(Option.ArrayBoundsChecks, false)]
            [Il2Cpp(Option.DivideByZeroChecks, false)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetComponent(in int index, in T value) => this.sharedComponents[this.ids[index]] = value;

            internal static ref ComponentsBag<T> Get(in int index) => ref cache[index];

            internal static int Create(World world) {
                var bag = new ComponentsBag<T> {
                    world            = world,
                    ids              = new int[1],
                    sharedComponents = CacheComponents<T>.Components
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

        public EntityEnumerator GetEnumerator() => new EntityEnumerator(this.world, this.entitiesCacheForBags, this.Length);

        IEnumerator<IEntity> IEnumerable<IEntity>.GetEnumerator() => new EntityEnumerator(this.world, this.entitiesCacheForBags, this.Length);

        IEnumerator IEnumerable.GetEnumerator() => new EntityEnumerator(this.world, this.entitiesCacheForBags, this.Length);

        public struct EntityEnumerator : IEnumerator<IEntity> {
            private World  world;
            private Entity current;
            private int[]  ids;

            private int id;
            private int length;

            internal EntityEnumerator(World world, int[] ids, int length) {
                this.world   = world;
                this.current = null;

                this.id     = -1;
                this.ids    = ids;
                this.length = length;
            }

            public bool MoveNext() {
                if (++this.id < this.length) {
                    this.current = this.world.Entities[this.ids[this.id]];
                    return true;
                }

                return false;
            }

            public void Reset() {
                this.current = null;
                this.id      = -1;
            }

            public IEntity Current => this.current;

            object IEnumerator.Current => this.current;

            public void Dispose() {
                this.id     = -1;
                this.length = -1;

                this.current = null;
                this.world   = null;
                this.ids     = null;
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
                Type     = typeof(T),
                GetBoxed = componentId => CacheComponents<T>.Components[componentId],
                SetBoxed = (componentId, value) => CacheComponents<T>.Components[componentId] = (T) value,
                Info     = CacheTypeIdentifier<T>.info
            };
            editorTypeAssociation.Add(id, info);
            return id;
        }

        internal struct DebugInfo {
            public Type                Type;
            public Func<int, object>   GetBoxed;
            public Action<int, object> SetBoxed;
            public TypeInfo            Info;
        }
#endif

        internal class TypeInfo {
            internal int         id;
            internal bool        isMarker;
            internal FastBitMask mask;

            public TypeInfo(bool isMarker) {
                this.isMarker = isMarker;
                this.mask     = new FastBitMask();
            }

            public void SetID(int id) {
                this.id = id;
                this.mask.SetBit(id);
            }
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal static class CacheTypeIdentifier<T> where T : struct, IComponent {
        internal static CommonCacheTypeIdentifier.TypeInfo info;

        static CacheTypeIdentifier() {
            info = new CommonCacheTypeIdentifier.TypeInfo(UnsafeUtility.SizeOf<T>() == 1);
#if UNITY_EDITOR
            var id = CommonCacheTypeIdentifier.GetID<T>();
#else
            var id = CommonCacheTypeIdentifier.GetID();
#endif
            info.SetID(id);
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal static class ComponentsCleaner {
        private static readonly Dictionary<int, RemoveDelegate> Cleaners;

        internal delegate bool RemoveDelegate(in int id);

        static ComponentsCleaner() => Cleaners = new Dictionary<int, RemoveDelegate>();

        internal static bool Register(in int typeId, RemoveDelegate func) {
            if (!Cleaners.ContainsKey(typeId)) {
                Cleaners.Add(typeId, func);
                return true;
            }

            return false;
        }


        internal static bool Clean(int typeId, int id) {
            if (Cleaners.TryGetValue(typeId, out var func)) {
                func(id);
                return true;
            }

            return false;
        }
    }

    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal static class CacheComponents<T> where T : struct, IComponent {
        internal static T[] Components;

        private static int capacity;
        private static int length;
        private static T   empty;

        private static readonly Queue<int> FreeIndexes;

        static CacheComponents() {
            capacity = Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY;
            length   = 0;

            Components  = new T[capacity];
            FreeIndexes = new Queue<int>();

            ComponentsCleaner.Register(CacheTypeIdentifier<T>.info.id, Remove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add() {
            var id = 0;

            if (FreeIndexes.Count > 0) {
                id = FreeIndexes.Dequeue();
                return id;
            }

            if (capacity <= length) {
                var newCapacity = capacity * 2;
                Array.Resize(ref Components, newCapacity);
                capacity = newCapacity;
            }

            id = length;
            length++;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get(in int id) => ref Components[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(in int id, in T value) => Components[id] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Empty() => ref empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove(in int id) {
            Components[id] = default;

            if (length >= id && !FreeIndexes.Contains(id)) {
                FreeIndexes.Enqueue(id);
                return true;
            }

            return false;
        }
    }

    namespace Utils {
        using System.Text;
        using FieldType = UInt64;

#pragma warning disable CS0659
#pragma warning disable CS0661
        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public struct FastBitMask : IEquatable<FastBitMask> {
            public static readonly FastBitMask None = new FastBitMask(new int[0]);

            private const int FIELD_COUNT          = 4;
            private const int BITS_PER_BYTE        = 8;
            private const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(ulong);
            private const int BITS_PER_FIELD_SHIFT = 6;

            private ulong field0;
            private ulong field1;
            private ulong field2;
            private ulong field3;

            public FastBitMask(int[] bits) {
                this.field0 = 0;
                this.field1 = 0;
                this.field2 = 0;
                this.field3 = 0;

                for (int i = 0, length = bits.Length; i < length; ++i) {
                    ref var bit = ref bits[i];

                    var dataIndex = bit >> BITS_PER_FIELD_SHIFT;
                    var bitIndex  = bit - (dataIndex << BITS_PER_FIELD_SHIFT);

                    var mask = (ulong) 1 << bitIndex;

                    switch (dataIndex) {
                        case 0:
                            this.field0 |= mask;
                            break;
                        case 1:
                            this.field1 |= mask;
                            break;
                        case 2:
                            this.field2 |= mask;
                            break;
                        case 3:
                            this.field3 |= mask;
                            break;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GetBit(in int index) {
                var dataIndex = index >> BITS_PER_FIELD_SHIFT;
                var bitIndex  = index - (dataIndex << BITS_PER_FIELD_SHIFT);

                switch (dataIndex) {
                    case 0:
                        return (this.field0 & ((ulong) 1 << bitIndex)) != 0;
                    case 1:
                        return (this.field1 & ((ulong) 1 << bitIndex)) != 0;
                    case 2:
                        return (this.field2 & ((ulong) 1 << bitIndex)) != 0;
                    case 3:
                        return (this.field3 & ((ulong) 1 << bitIndex)) != 0;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetBit(in int index) {
                var dataIndex = index >> BITS_PER_FIELD_SHIFT;
                var bitIndex  = index - (dataIndex << BITS_PER_FIELD_SHIFT);

                switch (dataIndex) {
                    case 0:
                        this.field0 |= (ulong) 1 << bitIndex;
                        break;
                    case 1:
                        this.field1 |= (ulong) 1 << bitIndex;
                        break;
                    case 2:
                        this.field2 |= (ulong) 1 << bitIndex;
                        break;
                    case 3:
                        this.field3 |= (ulong) 1 << bitIndex;
                        break;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FlipBit(in int index) {
                var dataIndex = index >> BITS_PER_FIELD_SHIFT;
                var bitIndex  = index - (dataIndex << BITS_PER_FIELD_SHIFT);

                switch (dataIndex) {
                    case 0:
                        this.field0 ^= (ulong) 1 << bitIndex;
                        break;
                    case 1:
                        this.field1 ^= (ulong) 1 << bitIndex;
                        break;
                    case 2:
                        this.field2 ^= (ulong) 1 << bitIndex;
                        break;
                    case 3:
                        this.field3 ^= (ulong) 1 << bitIndex;
                        break;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ClearBit(in int index) {
                var dataIndex = index >> BITS_PER_FIELD_SHIFT;
                var bitIndex  = index - (dataIndex << BITS_PER_FIELD_SHIFT);

                switch (dataIndex) {
                    case 0:
                        this.field0 &= ~((ulong) 1 << bitIndex);
                        break;
                    case 1:
                        this.field1 &= ~((ulong) 1 << bitIndex);
                        break;
                    case 2:
                        this.field2 &= ~((ulong) 1 << bitIndex);
                        break;
                    case 3:
                        this.field3 &= ~((ulong) 1 << bitIndex);
                        break;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetAll() {
                this.field0 = 0xffffffff;
                this.field1 = 0xffffffff;
                this.field2 = 0xffffffff;
                this.field3 = 0xffffffff;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ClearAll() {
                this.field0 = 0x00000000;
                this.field1 = 0x00000000;
                this.field2 = 0x00000000;
                this.field3 = 0x00000000;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(FastBitMask other) {
                if (this.field0 != other.field0) {
                    return false;
                }

                if (this.field1 != other.field0) {
                    return false;
                }

                if (this.field2 != other.field0) {
                    return false;
                }

                if (this.field3 != other.field0) {
                    return false;
                }

                return true;
            }

            public override bool Equals(object obj) {
                if (obj is FastBitMask mask) {
                    return this.Equals(mask);
                }

                return base.Equals(obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(FastBitMask mask1, FastBitMask mask2) => mask1.Equals(mask2);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(FastBitMask mask1, FastBitMask mask2) => !mask1.Equals(mask2);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static FastBitMask operator &(FastBitMask mask1, FastBitMask mask2) {
                var newBitMask = new FastBitMask();

                newBitMask.field0 = mask1.field0 & mask2.field0;
                newBitMask.field1 = mask1.field1 & mask2.field1;
                newBitMask.field2 = mask1.field2 & mask2.field2;
                newBitMask.field3 = mask1.field3 & mask2.field3;

                return newBitMask;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static FastBitMask operator |(FastBitMask mask1, FastBitMask mask2) {
                var newBitMask = new FastBitMask();

                newBitMask.field0 = mask1.field0 | mask2.field0;
                newBitMask.field1 = mask1.field1 | mask2.field1;
                newBitMask.field2 = mask1.field2 | mask2.field2;
                newBitMask.field3 = mask1.field3 | mask2.field3;

                return newBitMask;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static FastBitMask operator ~(FastBitMask mask) {
                var newBitMask = new FastBitMask();

                newBitMask.field0 = ~mask.field0;
                newBitMask.field1 = ~mask.field1;
                newBitMask.field2 = ~mask.field2;
                newBitMask.field3 = ~mask.field3;

                return newBitMask;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Has(in FastBitMask mask) {
                var temp = mask.field0;
                if ((this.field0 & temp) != temp) {
                    return false;
                }

                temp = mask.field1;
                if ((this.field1 & temp) != temp) {
                    return false;
                }

                temp = mask.field2;
                if ((this.field2 & temp) != temp) {
                    return false;
                }

                temp = mask.field3;
                if ((this.field3 & temp) != temp) {
                    return false;
                }

                return true;
            }

            public override string ToString() {
                var builder = new StringBuilder();

                var fields = new ulong[FIELD_COUNT];

                fields[0] = this.field0;
                fields[1] = this.field1;
                fields[2] = this.field2;
                fields[3] = this.field3;

                for (var i = 0; i < FIELD_COUNT; ++i) {
                    var binaryString = Convert.ToString((long) fields[i], 2);

                    builder.Append(binaryString.PadLeft(BITS_PER_FIELD, '0'));
                    builder.Append("_");
                }

                return builder.ToString();
            }
        }
#pragma warning restore CS0659
#pragma warning restore CS0661

        [Il2Cpp(Option.NullChecks, false)]
        [Il2Cpp(Option.ArrayBoundsChecks, false)]
        [Il2Cpp(Option.DivideByZeroChecks, false)]
        public class ObservableHashSet<T> : HashSet<T> {
            public Action<T> OnAddItem;
            public Action<T> OnRemoveItem;

            public new bool Add(T item) {
                var flag = base.Add(item);
                if (flag) {
                    this.OnAddItem?.Invoke(item);
                }

                return flag;
            }

            public new bool Remove(T item) {
                var flag = base.Remove(item);
                if (flag) {
                    this.OnRemoveItem?.Invoke(item);
                }

                return flag;
            }
        }

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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, Inherited = false,
        AllowMultiple                                                                                                                 = true)]
    public class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; }
        public object Value  { get; }

        public Il2CppSetOptionAttribute(Option option, object value) {
            this.Option = option;
            this.Value  = value;
        }
    }
}