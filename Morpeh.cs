[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests.Runtime")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Morpeh.Tests.Editor")]

namespace Morpeh {
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
#if UNITY_2019_1_OR_NEWER
    using JetBrains.Annotations;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using Object = UnityEngine.Object;
#endif
    using Utils;
    using Unity.IL2CPP.CompilerServices;
    using Il2Cpp = Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute;
    using System.Runtime.CompilerServices;

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
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int InternalID;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal FastBitMask ComponentsMask;
        internal World World => World.Worlds[this.worldID];
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private int worldID;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private int[] components;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private int componentsDoubleCount;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public int ID => this.InternalID;

        internal Entity(int id, int worldID) {
            this.InternalID = id;
            this.worldID    = worldID;

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
                return ref this.World.GetCache<T>().Empty();
            }

            this.ComponentsMask.SetBit(typeInfo.id);
            this.World.Filter.EntityChanged(this.InternalID);

            if (!typeInfo.isMarker) {
                var componentId = this.World.GetCache<T>().Add();
                for (int i = 0, length = this.componentsDoubleCount; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        this.components[i + 1] = componentId;
                        return ref this.World.GetCache<T>().Get(this.components[i + 1]);
                    }
                }

                this.componentsDoubleCount += 2;
                if (this.componentsDoubleCount >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCount << 1);
                }

                this.components[this.componentsDoubleCount - 2] = typeInfo.id;
                this.components[this.componentsDoubleCount - 1] = componentId;

                return ref this.World.GetCache<T>().Get(this.components[this.componentsDoubleCount - 1]);
            }

            return ref this.World.GetCache<T>().Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("You add component which already exist! Use Get or SetComponent instead!");
#endif
                exist = true;
                return ref this.World.GetCache<T>().Empty();
            }

            exist = false;
            this.ComponentsMask.SetBit(typeInfo.id);
            this.World.Filter.EntityChanged(this.InternalID);

            if (!typeInfo.isMarker) {
                var componentId = this.World.GetCache<T>().Add();
                for (int i = 0, length = this.componentsDoubleCount; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        this.components[i + 1] = componentId;
                        return ref this.World.GetCache<T>().Get(this.components[i + 1]);
                    }
                }

                this.componentsDoubleCount += 2;
                if (this.componentsDoubleCount >= this.components.Length) {
                    Array.Resize(ref this.components, this.componentsDoubleCount << 1);
                }

                this.components[this.componentsDoubleCount - 2] = typeInfo.id;
                this.components[this.componentsDoubleCount - 1] = componentId;

                return ref this.World.GetCache<T>().Get(this.components[this.componentsDoubleCount - 1]);
            }

            return ref this.World.GetCache<T>().Empty();
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetComponentId(int typeId) {
            var typeInfo = CommonCacheTypeIdentifier.editorTypeAssociation[typeId].TypeInfo;
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
                    return ref this.World.GetCache<T>().Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    return ref this.World.GetCache<T>().Get(this.components[i + 1]);
                }
            }

            return ref this.World.GetCache<T>().Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (typeInfo.isMarker) {
                if (this.Has<T>()) {
                    exist = true;
                    return ref this.World.GetCache<T>().Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    exist = true;
                    return ref this.World.GetCache<T>().Get(this.components[i + 1]);
                }
            }

            exist = false;
            return ref this.World.GetCache<T>().Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (!typeInfo.isMarker) {
                if (!this.Has(typeInfo.mask)) {
                    var componentId = this.World.GetCache<T>().Add();
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
                    this.World.GetCache<T>().Set(componentId, value);
                    this.ComponentsMask.SetBit(typeInfo.id);
                    return;
                }

                for (int i = 0, length = this.components.Length; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        this.World.GetCache<T>().Set(this.components[i + 1], value);
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
                        this.World.GetCache<T>().Remove(this.components[i + 1]);

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
                    this.World.GetCache(typeId).Remove(this.components[i + 1]);
                }

                mask.ClearBit(typeId);
            }

            this.ComponentsMask = FastBitMask.None;
        }
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    public sealed class SystemsGroup : IDisposable {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<ISystem> systems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<ISystem> fixedSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<ISystem> lateSystems;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<ISystem> disabledSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<ISystem> disabledFixedSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<ISystem> disabledLateSystems;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<IInitializer> newInitializers;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        private List<IDisposable> disposables;

        private World world;

        private SystemsGroup() {
            
        }
        
        internal SystemsGroup(World world) {
            this.world = world;
            
            this.systems      = new List<ISystem>();
            this.fixedSystems = new List<ISystem>();
            this.lateSystems  = new List<ISystem>();

            this.disabledSystems      = new List<ISystem>();
            this.disabledFixedSystems = new List<ISystem>();
            this.disabledLateSystems  = new List<ISystem>();

            this.newInitializers = new List<IInitializer>();
            this.disposables     = new List<IDisposable>();
        }

        public void Dispose() {
            void DisposeSystems(List<ISystem> systemsToDispose) {
                foreach (var system in systemsToDispose) {
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
        }

        public void Update(float deltaTime) {
            foreach (var disposable in this.disposables) {
                disposable.Dispose();
            }

            this.disposables.Clear();

            this.world.Filter.Update();

            foreach (var initializer in this.newInitializers) {
                initializer.OnAwake();
                this.world.Filter.Update();
            }

            this.newInitializers.Clear();

            for (int i = 0, length = this.systems.Count; i < length; i++) {
                this.systems[i].OnUpdate(deltaTime);
                this.world.Filter.Update();
            }
        }

        public void FixedUpdate(float deltaTime) {
            this.world.Filter.Update();

            for (int i = 0, length = this.fixedSystems.Count; i < length; i++) {
                this.fixedSystems[i].OnUpdate(deltaTime);
                this.world.Filter.Update();
            }
        }

        public void LateUpdate(float deltaTime) {
            this.world.Filter.Update();

            for (int i = 0, length = this.lateSystems.Count; i < length; i++) {
                this.lateSystems[i].OnUpdate(deltaTime);
                this.world.Filter.Update();
            }
        }

        public void AddInitializer<T>(T initializer) where T : class, IInitializer {
            initializer.World  = this.world;
            initializer.Filter = new FilterProvider {World = this.world};

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
    public sealed class World : IDisposable {
#if UNITY_2019_1_OR_NEWER
        [CanBeNull]
#endif
        public static World Default => Worlds[0];
#if UNITY_2019_1_OR_NEWER
        [NotNull]
#endif
        internal static List<World> Worlds = new List<World> {null};

        [NonSerialized]
        internal Filter Filter;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        internal SortedList<int, SystemsGroup> systemsGroups;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
        [HideInInspector]
#endif
        internal Entity[] Entities;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int EntitiesCount;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int EntitiesLength;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int EntitiesCapacity;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private List<int> freeEntityIDs;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal CacheComponents[] Caches;

        public static World Create() => new World().Initialize();

        private World() {
            this.Ctor();
        }

        internal void Ctor() {
            this.systemsGroups = new SortedList<int, SystemsGroup>();

            this.Filter = new Filter(this);
        }

        private World Initialize() {
#if UNITY_2019_1_OR_NEWER
            Worlds.Add(this);
#endif

            this.freeEntityIDs = new List<int>();
            this.Caches        = new CacheComponents[256];

            this.EntitiesLength   = 0;
            this.EntitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            this.Entities         = new Entity[this.EntitiesCapacity];

            return this;
        }

        public void Dispose() {
            foreach (var systemsGroup in this.systemsGroups.Values) {
                systemsGroup.Dispose();
            }

            this.systemsGroups = null;

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
#if UNITY_2019_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void InitializationDefaultWorld() {
            Worlds.Clear();
            Create();
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

        internal CacheComponents GetCache(int typeId) => this.Caches[typeId];

        internal CacheComponents<T> GetCache<T>() where T : struct, IComponent {
            var info  = CacheTypeIdentifier<T>.info;
            var cache = (CacheComponents<T>) this.Caches[info.id];
            if (cache == null) {
                this.Caches[info.id] = cache = new CacheComponents<T>();
            }

            return cache;
        }

        public static void GlobalUpdate(float deltaTime) {
            foreach (var world in Worlds) {
                foreach (var systemsGroup in world.systemsGroups.Values) {
                    systemsGroup.Update(deltaTime);
                }
            }
        }

        public static void GlobalFixedUpdate(float deltaTime) {
            foreach (var world in Worlds) {
                foreach (var systemsGroup in world.systemsGroups.Values) {
                    systemsGroup.FixedUpdate(deltaTime);
                }
            }
        }

        public static void GlobalLateUpdate(float deltaTime) {
            foreach (var world in Worlds) {
                foreach (var systemsGroup in world.systemsGroups.Values) {
                    systemsGroup.LateUpdate(deltaTime);
                }
            }
        }

        public SystemsGroup CreateSystemsGroup() => new SystemsGroup(this);
        
        public void AddSystemsGroup(int order, SystemsGroup systemsGroup) {
            this.systemsGroups.Add(order, systemsGroup);
        }
        
        public void RemoveSystemsGroup(SystemsGroup systemsGroup) {
            this.systemsGroups.RemoveAt(this.systemsGroups.IndexOfValue(systemsGroup));
        }
        
        public void RemoveAtSystemsGroup(int order) {
            this.systemsGroups.Remove(order);
        }

        public IEntity CreateEntity() => this.CreateEntityInternal();

        internal Entity CreateEntityInternal() {
            var id = -1;
            if (this.freeEntityIDs.Count > 0) {
                id = this.freeEntityIDs[0];
                this.freeEntityIDs.RemoveAtFast(0);
            }
            else {
                id = this.EntitiesLength++;
            }

            if (this.EntitiesLength >= this.EntitiesCapacity) {
                var newCapacity = this.EntitiesCapacity << 1;
                Array.Resize(ref this.Entities, newCapacity);
                this.EntitiesCapacity = newCapacity;
            }

            this.Entities[id] = new Entity(id, Worlds.IndexOf(this));
            this.Filter.Entities.Add(id);
            ++this.EntitiesCount;

            return this.Entities[id];
        }

        public IEntity CreateEntity(out int id) => this.CreateEntityInternal(out id);

        internal Entity CreateEntityInternal(out int id) {
            if (this.freeEntityIDs.Count > 0) {
                id = this.freeEntityIDs[0];
                this.freeEntityIDs.RemoveAtFast(0);
            }
            else {
                id = this.EntitiesLength++;
            }

            if (this.EntitiesLength >= this.EntitiesCapacity) {
                var newCapacity = this.EntitiesCapacity << 1;
                Array.Resize(ref this.Entities, newCapacity);
                this.EntitiesCapacity = newCapacity;
            }

            this.Entities[id] = new Entity(id, Worlds.IndexOf(this));
            this.Filter.Entities.Add(id);
            ++this.EntitiesCount;

            return this.Entities[id];
        }

        public IEntity GetEntity(in int id) => this.Entities[id];

        public void RemoveEntity(IEntity entity) {
            if (entity is Entity ent) {
                var id = ent.ID;
                if (this.Entities[id] == ent) {
                    this.freeEntityIDs.Add(id);
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

        private readonly World world;

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

#if UNITY_2019_1_OR_NEWER
        [CanBeNull]
#endif
        private List<int> addedList;
#if UNITY_2019_1_OR_NEWER
        [CanBeNull]
#endif
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEntity First() => this.world.Entities[this.entitiesCacheForBags[0]];

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
                    sharedComponents = world.GetCache<T>().Components
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
                GetBoxed = (world, componentId) => world.GetCache<T>().Components[componentId],
                SetBoxed = (world, componentId, value) => world.GetCache<T>().Components[componentId] = (T) value,
                TypeInfo = CacheTypeIdentifier<T>.info
            };
            editorTypeAssociation.Add(id, info);
            return id;
        }

        internal struct DebugInfo {
            public Type                       Type;
            public Func<World, int, object>   GetBoxed;
            public Action<World, int, object> SetBoxed;
            public TypeInfo                   TypeInfo;
        }
#endif

        [Serializable]
        internal class TypeInfo {
#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
            internal int id;
#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
            internal bool isMarker;
#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
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
    internal abstract class CacheComponents {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool Remove(in int id);
    }

    [Serializable]
    [Il2Cpp(Option.NullChecks, false)]
    [Il2Cpp(Option.ArrayBoundsChecks, false)]
    [Il2Cpp(Option.DivideByZeroChecks, false)]
    internal sealed class CacheComponents<T> : CacheComponents where T : struct, IComponent {
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal T[] Components;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private int capacity;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private int length;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private T empty;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private List<int> freeIndexes;

        public CacheComponents() {
            this.capacity = Constants.DEFAULT_CACHE_COMPONENTS_CAPACITY;
            this.length   = 0;

            this.Components  = new T[this.capacity];
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
                Array.Resize(ref this.Components, newCapacity);
                this.capacity = newCapacity;
            }

            id = this.length;
            this.length++;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(in int id) => ref this.Components[id];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(in int id, in T value) => this.Components[id] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Empty() => ref this.empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Remove(in int id) {
            this.Components[id] = default;

            if (this.length >= id && !this.freeIndexes.Contains(id)) {
                this.freeIndexes.Add(id);
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
        [Serializable]
        public struct FastBitMask : IEquatable<FastBitMask> {
            public static readonly FastBitMask None = new FastBitMask(new int[0]);

            private const int FIELD_COUNT          = 4;
            private const int BITS_PER_BYTE        = 8;
            private const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(ulong);
            private const int BITS_PER_FIELD_SHIFT = 6;

#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
            private ulong field0;
#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
            private ulong field1;
#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
            private ulong field2;
#if UNITY_2019_1_OR_NEWER
            [SerializeField]
#endif
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; }
        public object Value  { get; }

        public Il2CppSetOptionAttribute(Option option, object value) {
            this.Option = option;
            this.Value  = value;
        }
    }
}