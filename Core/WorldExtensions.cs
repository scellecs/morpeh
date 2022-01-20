#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static partial class WorldExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Ctor(this World world) {
            world.systemsGroups    = new SortedList<int, SystemsGroup>();
            world.newSystemsGroups = new SortedList<int, SystemsGroup>();

            world.Filter         = new Filter(world);
            world.filters        = new FastList<Filter>();
            world.archetypeCache = new IntFastList();
            world.dirtyEntities  = new BitMap();

            if (world.archetypes != null) {
                foreach (var archetype in world.archetypes) {
                    archetype.Ctor();
                }
            }

            world.InitializeGlobals();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static partial void InitializeGlobals(this World world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static World Initialize(this World world) {
            World.worlds.Add(world);
            world.identifier        = World.worlds.length - 1;
            world.freeEntityIDs     = new IntFastList();
            world.nextFreeEntityIDs = new IntFastList();
            world.caches            = new UnsafeIntHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);
            world.typedCaches       = new UnsafeIntHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);

            world.entitiesCount    = 0;
            world.entitiesLength   = 0;
            world.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            world.entities         = new Entity[world.entitiesCapacity];

            world.archetypes         = new FastList<Archetype> { new Archetype(0, Array.Empty<int>(), world.identifier) };
            world.archetypesByLength = new IntHashMap<IntFastList>();
            world.archetypesByLength.Add(0, new IntFastList { 0 }, out _);
            world.newArchetypes = new IntFastList();

            return world;
        }

#if UNITY_2019_1_OR_NEWER && !MORPEH_DISABLE_AUTOINITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void InitializationDefaultWorld() {
            ComponentsCache.cleanup();

            World.worlds.Clear();
            var defaultWorld = World.Create("Default World");
            defaultWorld.UpdateByUnity = true;
#if UNITY_2019_1_OR_NEWER
            var go = new GameObject {
                name      = "MORPEH_UNITY_RUNTIME_HELPER",
                hideFlags = HideFlags.DontSaveInEditor
            };
            go.AddComponent<UnityRuntimeHelper>();
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
#endif
            //Warm Types
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (typeof(IComponent).IsAssignableFrom(type) && type.IsValueType && !type.ContainsGenericParameters) {
                        try {
                            typeof(TypeIdentifier<>)
                                .MakeGenericType(type)
                                .GetMethod("Warmup", BindingFlags.Static | BindingFlags.Public)
                                .Invoke(null, null);
                        }
                        catch {
                            MDebug.LogWarning($"Attention component type {type.FullName} not used, but exists in build");
                        }
                    }
                }
            }
        }

        //TODO refactor allocations and fast sort(maybe without it?)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Archetype GetArchetype(this World world, int[] typeIds, int newTypeId, bool added, out int archetypeId) {
            Archetype archetype = null;
            archetypeId = -1;

            world.archetypeCache.Clear();
            for (int i = 0, length = typeIds.Length; i < length; i++) {
                var typeId = typeIds[i];
                if (typeId >= 0) {
                    world.archetypeCache.Add(typeIds[i]);
                }
            }

            if (added) {
                world.archetypeCache.Add(newTypeId);
            }
            else {
                world.archetypeCache.Remove(newTypeId);
            }

            world.archetypeCache.Sort();
            var typesLength = world.archetypeCache.length;

            if (world.archetypesByLength.TryGetValue(typesLength, out var archetypesList)) {
                for (var index = 0; index < archetypesList.length; index++) {
                    archetypeId = archetypesList.Get(index);
                    archetype   = world.archetypes.data[archetypeId];
                    var check = true;
                    for (int i = 0, length = typesLength; i < length; i++) {
                        if (archetype.typeIds[i] != world.archetypeCache.Get(i)) {
                            check = false;
                            break;
                        }
                    }

                    if (check) {
                        return archetype;
                    }
                }
            }

            archetypeId = world.archetypes.length;
            var newArchetype = new Archetype(archetypeId, world.archetypeCache.ToArray(), world.identifier);
            world.archetypes.Add(newArchetype);
            if (world.archetypesByLength.TryGetValue(typesLength, out archetypesList)) {
                archetypesList.Add(archetypeId);
            }
            else {
                world.archetypesByLength.Add(typesLength, new IntFastList { archetypeId }, out _);
            }

            world.newArchetypes.Add(archetypeId);

            archetype = newArchetype;

            return archetype;
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ComponentsCache GetCache(this World world, int typeId) {
            if (world.caches.TryGetValue(typeId, out var index)) {
                return ComponentsCache.caches.data[index];
            }

            return null;
        }

        public static ComponentsCache<T> GetCache<T>(this World world) where T : struct, IComponent {
            var info = TypeIdentifier<T>.info;
            if (world.typedCaches.TryGetValue(info.id, out var typedIndex)) {
                return ComponentsCache<T>.typedCaches.data[typedIndex];
            }

            ComponentsCache<T> componentsCache;
            if (info.isDisposable) {
                var constructedType = typeof(ComponentsCacheDisposable<>).MakeGenericType(typeof(T));
                componentsCache = (ComponentsCache<T>)Activator.CreateInstance(constructedType);
            }
            else {
                componentsCache = new ComponentsCache<T>();
            }

            world.caches.Add(info.id, componentsCache.commonCacheId, out _);
            world.typedCaches.Add(info.id, componentsCache.typedCacheId, out _);

            return componentsCache;
        }

        public static void GlobalUpdate(float deltaTime) {
            foreach (var world in World.worlds) {
                if (world.UpdateByUnity) {
                    world.Update(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(this World world, float deltaTime) {
            for (var i = 0; i < world.newSystemsGroups.Count; i++) {
                var key          = world.newSystemsGroups.Keys[i];
                var systemsGroup = world.newSystemsGroups.Values[i];

                systemsGroup.Initialize();
                world.systemsGroups.Add(key, systemsGroup);
            }

            world.newSystemsGroups.Clear();

            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.Update(deltaTime);
            }
        }

        public static void GlobalFixedUpdate(float deltaTime) {
            foreach (var world in World.worlds) {
                if (world.UpdateByUnity) {
                    world.FixedUpdate(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FixedUpdate(this World world, float deltaTime) {
            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.FixedUpdate(deltaTime);
            }
        }

        public static void GlobalLateUpdate(float deltaTime) {
            foreach (var world in World.worlds) {
                if (world.UpdateByUnity) {
                    world.LateUpdate(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LateUpdate(this World world, float deltaTime) {
            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.LateUpdate(deltaTime);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SystemsGroup CreateSystemsGroup(this World world) => new SystemsGroup(world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSystemsGroup(this World world, int order, SystemsGroup systemsGroup) {
            world.newSystemsGroups.Add(order, systemsGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSystemsGroup(this World world, SystemsGroup systemsGroup) {
            systemsGroup.Dispose();
            if (world.systemsGroups.ContainsValue(systemsGroup)) {
                world.systemsGroups.RemoveAt(world.systemsGroups.IndexOfValue(systemsGroup));
            }
            else if (world.newSystemsGroups.ContainsValue(systemsGroup)) {
                world.newSystemsGroups.RemoveAt(world.newSystemsGroups.IndexOfValue(systemsGroup));
            }
        }

        public static Entity CreateEntity(this World world) {
            int id;
            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Get(0);
                world.freeEntityIDs.RemoveAtSwap(0, out _);
            }
            else {
                id = world.entitiesLength++;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                var newCapacity = HashHelpers.ExpandCapacity(world.entitiesCapacity) + 1;
                Array.Resize(ref world.entities, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, World.worlds.IndexOf(world));
            ++world.entitiesCount;
            
            return world.entities[id];
        }

        public static Entity CreateEntity(this World world, out int id) {
            
            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Get(0);
                world.freeEntityIDs.RemoveAtSwap(0, out _);
            }
            else {
                id = world.entitiesLength++;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                var newCapacity = HashHelpers.ExpandCapacity(world.entitiesCapacity) + 1;
                Array.Resize(ref world.entities, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, World.worlds.IndexOf(world));
            ++world.entitiesCount;
            
            return world.entities[id];
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this World world, in int id) => world.entities[id];

        public static void RemoveEntity(this World world, Entity entity) {
            if (world.entities[entity.internalID] == entity) {
                entity.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyRemoveEntity(this World world, int id) {
            world.nextFreeEntityIDs.Add(id);
            world.entities[id] = null;
            --world.entitiesCount;
        }

        public static void UpdateFilters(this World world) {
            foreach (var entityId in world.dirtyEntities) {
                world.entities[entityId]?.ApplyTransfer();
            }

            world.dirtyEntities.Clear();

            if (world.newArchetypes.length > 0) {
                for (var index = 0; index < world.filters.length; index++) {
                    world.filters.data[index].FindArchetypes(world.newArchetypes);
                }

                world.newArchetypes.Clear();
            }

            for (var index = 0; index < world.archetypes.length; index++) {
                var archetype = world.archetypes.data[index];
                if (archetype.isDirty) {
                    archetype.Process();
                }
            }

            for (int index = 0, length = world.filters.length; index < length; index++) {
                var filter = world.filters.data[index];
                if (filter.isDirty) {
                    filter.UpdateLength();
                }
            }

            if (world.nextFreeEntityIDs.length > 0) {
                world.freeEntityIDs.AddListRange(world.nextFreeEntityIDs);
                world.nextFreeEntityIDs.Clear();
            }
        }

        public static void SetFriendlyName(this World world, string friendlyName) {
            world.friendlyName = friendlyName;
        }

        public static string GetFriendlyName(this World world) {
            if (string.IsNullOrEmpty(world.friendlyName)) {
                return world.ToString() + world.identifier;
            }

            return world.friendlyName;
        }
    }
}
