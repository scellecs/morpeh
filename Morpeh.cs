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
    using System.Linq;
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
        internal const int DEFAULT_WORLD_CACHES_CAPACITY               = 256;
        internal const int DEFAULT_ENTITY_COMPONENTS_CAPACITY          = 2;
        internal const int DEFAULT_ROOT_FILTER_DIRTY_ENTITIES_CAPACITY = DEFAULT_WORLD_ENTITIES_CAPACITY;
        internal const int DEFAULT_FILTER_DIRTY_ENTITIES_CAPACITY      = 1024;
        internal const int DEFAULT_FILTER_ADDED_ENTITIES_CAPACITY      = 32;
        internal const int DEFAULT_FILTER_REMOVED_ENTITIES_CAPACITY    = 32;
        internal const int DEFAULT_CACHE_COMPONENTS_CAPACITY           = 2048;
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
        internal World World => World.worlds[this.worldID];

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int internalID;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int worldID;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int[] components;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int componentsDoubleCapacity;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int componentsCount;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal bool isDirty;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private bool isDisposed;


#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public int ID => this.internalID;

        internal Entity(int id, int worldID) {
            this.internalID = id;
            this.worldID    = worldID;

            this.componentsDoubleCapacity = 0;
            this.componentsCount          = 0;

            this.components = new int[Constants.DEFAULT_ENTITY_COMPONENTS_CAPACITY];
            for (int i = 0, length = this.components.Length; i < length; i++) {
                this.components[i] = -1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
                return ref CacheComponents<T>.Empty();
            }

            this.MakeDirty();

            if (typeInfo.isMarker) {
                const int componentId = -1;
                for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                    if (this.components[i] == -1) {
                        this.components[i]     = typeInfo.id;
                        this.components[i + 1] = componentId;
                        this.componentsCount++;

                        return ref CacheComponents<T>.Empty();
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

                return ref this.World.GetCache<T>().Get(this.components[this.componentsDoubleCapacity - 1]);
            }

            return ref CacheComponents<T>.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (this.Has<T>()) {
#if UNITY_EDITOR
                Debug.LogError("You're trying to add a component that already exists! Use Get or SetComponent instead!");
#endif
                exist = true;
                return ref CacheComponents<T>.Empty();
            }

            exist = false;
            this.MakeDirty();

            if (typeInfo.isMarker) {
                const int componentId = -1;
                for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                    if (this.components[i] == -1) {
                        this.components[i]     = typeInfo.id;
                        this.components[i + 1] = componentId;
                        this.componentsCount++;

                        return ref CacheComponents<T>.Empty();
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

                return ref this.World.GetCache<T>().Get(this.components[this.componentsDoubleCapacity - 1]);
            }

            return ref CacheComponents<T>.Empty();
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

            if (typeInfo.isMarker) {
                if (this.Has<T>()) {
                    return ref CacheComponents<T>.Empty();
                }
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    return ref this.World.GetCache<T>().Get(this.components[i + 1]);
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
                    return ref this.World.GetCache<T>().Get(this.components[i + 1]);
                }
            }

            exist = false;
            return ref CacheComponents<T>.Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            if (!typeInfo.isMarker) {
                if (!this.Has(typeInfo.id)) {
                    var componentId = this.World.GetCache<T>().Add();

                    for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                        if (this.components[i] == -1) {
                            this.components[i]     = typeInfo.id;
                            this.components[i + 1] = componentId;
                            this.componentsCount++;

                            this.World.GetCache<T>().Set(componentId, value);
                            this.MakeDirty();
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

                    this.World.GetCache<T>().Set(componentId, value);
                    this.MakeDirty();
                    return;
                }

                for (int i = 0, length = this.components.Length; i < length; i += 2) {
                    if (this.components[i] == typeInfo.id) {
                        this.World.GetCache<T>().Set(this.components[i + 1], value);
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

                            this.MakeDirty();
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
                    this.MakeDirty();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponent {
            var typeInfo = CacheTypeIdentifier<T>.info;

            for (int i = 0, length = this.componentsDoubleCapacity; i < length; i += 2) {
                if (this.components[i] == typeInfo.id) {
                    this.components[i] = -1;
                    if (!typeInfo.isMarker) {
                        this.World.GetCache<T>().Remove(this.components[i + 1]);
                        this.components[i + 1] = -1;
                    }

                    --this.componentsCount;

                    if (this.componentsCount <= 0) {
                        this.World.RemoveEntity(this);
                    }
                    else {
                        this.MakeDirty();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MakeDirty() {
            if (this.isDirty) {
                return;
            }

            this.isDirty = true;
            this.World.Filter.EntityChanged(this.internalID);
        }

        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            for (int i = 0, length = this.components.Length; i < length; i += 2) {
                var typeId = this.components[i];
                if (typeId >= 0) {
                    this.World.GetCache(typeId)?.Remove(this.components[i + 1]);
                }
            }

            this.components               = null;
            this.internalID               = -1;
            this.worldID                  = -1;
            this.componentsDoubleCapacity = -1;
            this.componentsCount          = -1;

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
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<ISystem> systems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<ISystem> fixedSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<ISystem> lateSystems;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<ISystem> disabledSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<ISystem> disabledFixedSystems;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<ISystem> disabledLateSystems;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<IInitializer> newInitializers;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private List<IInitializer> initializers;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
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
            this.initializers    = new List<IInitializer>();
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

            foreach (var initializer in this.initializers) {
                initializer.Dispose();
            }

            this.initializers.Clear();
            this.initializers = null;

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
                this.initializers.Add(initializer);
            }

            this.newInitializers.Clear();

            for (int i = 0, length = this.systems.Count; i < length; i++) {
                this.systems[i].OnUpdate(deltaTime);
                this.world.Filter.Update();
            }
        }

        public void FixedUpdate(float deltaTime) {
            foreach (var disposable in this.disposables) {
                disposable.Dispose();
            }

            this.disposables.Clear();

            this.world.Filter.Update();

            foreach (var initializer in this.newInitializers) {
                initializer.OnAwake();
                this.world.Filter.Update();
                this.initializers.Add(initializer);
            }

            this.newInitializers.Clear();

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
#if UNITY_2019_1_OR_NEWER
        [CanBeNull]
#endif
        public static World Default => worlds[0];
#if UNITY_2019_1_OR_NEWER
        [NotNull]
#endif
        internal static List<World> worlds = new List<World> {null};

        [NonSerialized]
        public Filter Filter;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        [NonSerialized]
        internal SortedList<int, SystemsGroup> systemsGroups;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal Entity[] entities;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int entitiesCount;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int entitiesLength;
#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal int entitiesCapacity;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        private List<int> freeEntityIDs;

#if UNITY_2019_1_OR_NEWER
        [SerializeField]
#endif
        internal CacheComponents[] caches;

        public static World Create() => new World().Initialize();

        private World() {
            this.Ctor();
        }

        internal void Ctor() {
            this.systemsGroups = new SortedList<int, SystemsGroup>();

            this.Filter = new Filter(this);

            this.InitializeGlobals();
        }

        partial void InitializeGlobals();

        private World Initialize() {
#if UNITY_2019_1_OR_NEWER
            worlds.Add(this);
#endif
            this.freeEntityIDs = new List<int>();
            this.caches        = new CacheComponents[Constants.DEFAULT_WORLD_CACHES_CAPACITY];

            this.entitiesLength   = 0;
            this.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            this.entities         = new Entity[this.entitiesCapacity];

            return this;
        }

        public void Dispose() {
            foreach (var systemsGroup in this.systemsGroups.Values) {
                systemsGroup.Dispose();
            }

            this.systemsGroups = null;

            foreach (var entity in this.entities) {
                entity?.Dispose();
            }

            this.entities         = null;
            this.entitiesLength   = -1;
            this.entitiesCapacity = -1;

            this.freeEntityIDs.Clear();
            this.freeEntityIDs = null;

            this.Filter.Dispose();
            this.Filter = null;

            foreach (var cache in this.caches) {
                cache?.Dispose();
            }

            this.caches = null;

            worlds.Remove(this);
        }
#if UNITY_2019_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void InitializationDefaultWorld() {
            worlds.Clear();
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
                for (var i = 0; i < world.systemsGroups.Values.Count; i++) {
                    var systemsGroup = world.systemsGroups.Values[i];
                    systemsGroup.Update(deltaTime);
                }
            }
        }

        public static void GlobalFixedUpdate(float deltaTime) {
            foreach (var world in worlds) {
                for (var i = 0; i < world.systemsGroups.Values.Count; i++) {
                    var systemsGroup = world.systemsGroups.Values[i];
                    systemsGroup.FixedUpdate(deltaTime);
                }
            }
        }

        public static void GlobalLateUpdate(float deltaTime) {
            foreach (var world in worlds) {
                for (var i = 0; i < world.systemsGroups.Values.Count; i++) {
                    var systemsGroup = world.systemsGroups.Values[i];
                    systemsGroup.LateUpdate(deltaTime);
                }
            }
        }

        public SystemsGroup CreateSystemsGroup() => new SystemsGroup(this);

        public void AddSystemsGroup(int order, SystemsGroup systemsGroup) {
            this.systemsGroups.Add(order, systemsGroup);
        }

        public void AddLastSystemsGroup(SystemsGroup systemsGroup) {
            this.systemsGroups.Add(this.systemsGroups.Last().Key + 1, systemsGroup);
        }

        public void RemoveSystemsGroup(SystemsGroup systemsGroup) {
            systemsGroup.Dispose();
            this.systemsGroups.RemoveAt(this.systemsGroups.IndexOfValue(systemsGroup));
        }

        public void RemoveAtSystemsGroup(int order) {
            this.systemsGroups[order].Dispose();
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
                id = this.entitiesLength++;
            }

            if (this.entitiesLength >= this.entitiesCapacity) {
                var newCapacity = this.entitiesCapacity << 1;
                Array.Resize(ref this.entities, newCapacity);
                this.entitiesCapacity = newCapacity;
            }

            this.entities[id] = new Entity(id, worlds.IndexOf(this));
            this.Filter.entities.Add(id);
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
            this.Filter.entities.Add(id);
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
                    this.freeEntityIDs.Add(id);
                    this.Filter.entities.Remove(id);
                    this.entities[id] = null;
                    --this.entitiesCount;
                    ent.Dispose();
                }
            }
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

        public int Length;

        internal ObservableHashSet<int> entities;

        private readonly World world;

        private int[] entitiesCacheForBags;
        private int   entitiesCacheForBagsCapacity;

        // 0 - typeIndex
        // 1 - componentsBagCacheIndex
        // 2 - isDirty
        private int[] componentsBags;
        private int   componentsBagsTripleCount;

        private List<Filter> childs;
        private int          typeID;
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

        private int lastEntitiesCount = -1;

        private bool isDirtyCache = true;

        //root filter ctor
        //don't allocate any trash
        internal Filter(World world) {
            this.world = world;

            this.entities  = new ObservableHashSet<int>();
            this.dirtyList = new List<int>(Constants.DEFAULT_ROOT_FILTER_DIRTY_ENTITIES_CAPACITY);
            this.childs    = new List<Filter>();

            this.typeID     = -1;
            this.filterMode = FilterMode.Include;

            this.entitiesCacheForBags         = new int[0];
            this.entitiesCacheForBagsCapacity = 0;
        }


        //full child filter
        private Filter(World world, ObservableHashSet<int> rootEntities, int typeID, FilterMode mode, bool fillWithPreviousEntities) {
            this.world = world;

            this.addedList   = new List<int>(Constants.DEFAULT_FILTER_ADDED_ENTITIES_CAPACITY);
            this.removedList = new List<int>(Constants.DEFAULT_FILTER_REMOVED_ENTITIES_CAPACITY);
            this.entities    = new ObservableHashSet<int>();
            this.dirtyList   = new List<int>(Constants.DEFAULT_FILTER_DIRTY_ENTITIES_CAPACITY);
            this.childs      = new List<Filter>();

            this.typeID     = typeID;
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
                var entity = this.world.GetEntityInternal(id);
                switch (this.filterMode) {
                    case FilterMode.Include:
                        if (entity != null && !entity.IsDisposed() && entity.Has(this.typeID)) {
                            this.entities.Add(id);
                        }

                        break;
                    case FilterMode.Exclude:
                        if (entity != null && !entity.IsDisposed() && !entity.Has(this.typeID)) {
                            this.entities.Add(id);
                        }

                        break;
                }
            }

            this.Length = this.entities.Count;
        }

        public void Dispose() {
            foreach (var child in this.childs) {
                child.Dispose();
            }

            this.childs.Clear();
            this.childs = null;

            this.Length = -1;

            this.entities.Clear();
            this.entities = null;

            this.entitiesCacheForBags         = null;
            this.entitiesCacheForBagsCapacity = -1;

            this.componentsBags            = null;
            this.componentsBagsTripleCount = -1;

            this.typeID     = -1;
            this.filterMode = FilterMode.None;

            this.addedList?.Clear();
            this.addedList = null;
            this.removedList?.Clear();
            this.removedList = null;

            this.dirtyList.Clear();
            this.dirtyList = null;

            this.lastEntitiesCount = -1;
        }

        internal void EntityChanged(int id) => this.dirtyList.Add(id);

        public void Update() {
            if (this.typeID == -1) {
                var entitiesCount = this.entities.Count;
                if (this.lastEntitiesCount != entitiesCount || this.dirtyList.Count > 0) {
                    for (int i = 0, length = this.childs.Count; i < length; i++) {
                        this.childs[i].UpdateChilds(this.dirtyList);
                    }

                    this.lastEntitiesCount = entitiesCount;

                    for (int i = 0, length = this.dirtyList.Count; i < length; i++) {
                        var entity = this.world.GetEntityInternal(this.dirtyList[i]);
                        if (!entity.IsNullOrDisposed()) {
                            entity.isDirty = false;
                        }
                    }

                    this.dirtyList.Clear();
                }

                return;
            }

            var originDirtyCount = this.dirtyList.Count;

            for (var i = this.dirtyList.Count - 1; i >= 0; i--) {
                var dirtyId = this.dirtyList[i];
                var entity  = this.world.GetEntityInternal(dirtyId);
                if (entity.IsNullOrDisposed()) {
                    this.entities.Remove(dirtyId);
                    this.dirtyList.RemoveAtFast(i);
                }
                else if (this.filterMode == FilterMode.Include) {
                    if (entity.Has(this.typeID)) {
                        if (this.entities.Add(dirtyId)) {
                            this.dirtyList.RemoveAtFast(i);
                        }
                    }
                    else {
                        this.entities.Remove(dirtyId);
                        this.dirtyList.RemoveAtFast(i);
                    }
                }
                else if (this.filterMode == FilterMode.Exclude) {
                    if (!entity.Has(this.typeID)) {
                        if (this.entities.Add(dirtyId)) {
                            this.dirtyList.RemoveAtFast(i);
                        }
                    }
                    else {
                        this.entities.Remove(dirtyId);
                        this.dirtyList.RemoveAtFast(i);
                    }
                }
            }

            if (this.addedList.Count > 0 || this.removedList.Count > 0 || originDirtyCount != this.dirtyList.Count) {
                for (int i = 2, length = this.componentsBagsTripleCount; i < length; i += 3) {
                    this.componentsBags[i] = 1;
                }

                this.isDirtyCache = true;
            }

            for (int i = 0, length = this.removedList.Count; i < length; i++) {
                var id = this.removedList[i];
                this.entities.Remove(id);
            }

            this.removedList.Clear();

            for (int i = 0, length = this.addedList.Count; i < length; i++) {
                var id     = this.addedList[i];
                var entity = this.world.GetEntityInternal(id);
                switch (this.filterMode) {
                    case FilterMode.Include:
                        if (entity != null && !entity.IsDisposed() && entity.Has(this.typeID)) {
                            this.entities.Add(id);
                        }

                        break;
                    case FilterMode.Exclude:
                        if (entity != null && !entity.IsDisposed() && !entity.Has(this.typeID)) {
                            this.entities.Add(id);
                        }

                        break;
                }
            }

            this.addedList.Clear();

            for (int i = 0, length = this.childs.Count; i < length; i++) {
                this.childs[i].UpdateChilds(this.dirtyList);
            }

            this.dirtyList.Clear();

            this.Length = this.entities.Count;
        }

        private void UpdateChilds(List<int> parentDirtyList) {
            this.dirtyList.AddRange(parentDirtyList);
            this.Update();
        }

        private void UpdateCache() {
            if (this.entitiesCacheForBagsCapacity < this.Length) {
                Array.Resize(ref this.entitiesCacheForBags, this.Length);
                this.entitiesCacheForBagsCapacity = this.Length;
            }

            var i = 0;
            foreach (var id in this.entities) {
                this.entitiesCacheForBags[i++] = id;
            }

            this.isDirtyCache = true;
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

        public Filter With<T>(bool fillWithPreviousEntities = true) where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Include, fillWithPreviousEntities);

        public Filter Without<T>(bool fillWithPreviousEntities = true) where T : struct, IComponent
            => this.CreateFilter<T>(FilterMode.Exclude, fillWithPreviousEntities);

        private Filter CreateFilter<T>(FilterMode mode, bool fillWithPreviousEntities) where T : struct, IComponent {
            for (int i = 0, length = this.childs.Count; i < length; i++) {
                var child = this.childs[i];
                if (child.filterMode == mode && child.typeID == CacheTypeIdentifier<T>.info.id) {
                    return child;
                }
            }

            var newFilter = new Filter(this.world, this.entities, CacheTypeIdentifier<T>.info.id, mode, fillWithPreviousEntities);
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
                    this.ids[i] = this.world.entities[entities[i]].GetComponentId<T>();
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

        public EntityEnumerator GetEnumerator() {
            if (this.isDirtyCache) {
                this.UpdateCache();
            }
            return new EntityEnumerator(this.world, this.entitiesCacheForBags, this.Length);
        }

        IEnumerator<IEntity> IEnumerable<IEntity>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

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
                    this.current = this.world.entities[this.ids[this.id]];
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
                type     = typeof(T),
                getBoxed = (world, componentId) => world.GetCache<T>().Components[componentId],
                setBoxed = (world, componentId, value) => world.GetCache<T>().Components[componentId] = (T) value,
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
        private static T empty;

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
        public static ref T Empty() => ref empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Remove(in int id) {
            this.Components[id] = default;

            if (this.length >= id && !this.freeIndexes.Contains(id)) {
                this.freeIndexes.Add(id);
                return true;
            }

            return false;
        }

        public override void Dispose() {
            this.Components = null;
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
            this.Components[id].Dispose();
            return base.Remove(in id);
        }
    }

    namespace Utils {
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