#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Morpeh;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class WorldExtensions {
        internal static FastList<IWorldPlugin> plugins;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Ctor(this World world) {
            world.threadIdLock = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            world.systemsGroups    = new SortedList<int, SystemsGroup>();
            world.newSystemsGroups = new SortedList<int, SystemsGroup>();

            world.pluginSystemsGroups    = new FastList<SystemsGroup>();
            world.newPluginSystemsGroups = new FastList<SystemsGroup>();

            world.Filter         = new Filter(world);
            world.filters        = new FastList<Filter>();
            world.archetypeCache = new IntFastList();
            world.dirtyEntities  = new BitMap();

            if (world.archetypes != null) {
                foreach (var archetype in world.archetypes) {
                    archetype.Ctor();
                }
            }

            Warmup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static World Initialize(this World world) {
            var added = false;
            var id    = -1;

            for (int i = 0, length = World.worlds.length; i < length; i++) {
                if (World.worlds.data[i] == null) {
                    added                = true;
                    id                   = i;
                    World.worlds.data[i] = world;
                    break;
                }
            }
            if (added == false) {
                World.worlds.Add(world);
            }
            world.identifier        = added ? id : World.worlds.length - 1;
            world.freeEntityIDs     = new IntFastList();
            world.nextFreeEntityIDs = new IntFastList();
            world.stashes           = new UnsafeIntHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);
            world.typedStashes      = new UnsafeIntHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);

            world.entitiesCount    = 0;
            world.entitiesLength   = 0;
            world.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            world.entities         = new Entity[world.entitiesCapacity];
            world.entitiesGens     = new int[world.entitiesCapacity];

            world.archetypes         = new FastList<Archetype> { new Archetype(0, Array.Empty<int>(), world.identifier) };
            world.archetypesByLength = new IntHashMap<IntFastList>();
            world.archetypesByLength.Add(0, new IntFastList { 0 }, out _);
            world.newArchetypes = new IntFastList();
            
            foreach (var worldPlugin in plugins) {
                worldPlugin.Initialize(world);
            }

            return world;
        }

#if UNITY_2019_1_OR_NEWER && !MORPEH_DISABLE_AUTOINITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void InitializationDefaultWorld() {
            Stash.cleanup();

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
            Warmup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Warmup() {
            if (plugins != null) {
                return;
            }

            plugins = new FastList<IWorldPlugin>();
            var componentType = typeof(IComponent);
            var pluginType    = typeof(IWorldPlugin);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (componentType.IsAssignableFrom(type) && type.IsValueType && !type.ContainsGenericParameters) {
                        try {
                            typeof(TypeIdentifier<>)
                                .MakeGenericType(type)
                                .GetMethod("Warmup", BindingFlags.Static | BindingFlags.Public)
                                .Invoke(null, null);
                        }
                        catch {
                            MLogger.LogWarning($"Attention component type {type.FullName} not used, but exists in build");
                        }
                    }
                    else if (pluginType.IsAssignableFrom(type) && !type.IsValueType && !type.ContainsGenericParameters && pluginType != type) {
                        var instance = (IWorldPlugin)Activator.CreateInstance(type);
                        plugins.Add(instance);
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
        internal static Stash GetStash(this World world, int typeId) {
            if (world.stashes.TryGetValue(typeId, out var index)) {
                return Stash.stashes.data[index];
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stash GetReflectionStash(this World world, Type type) {
            world.ThreadSafetyCheck();
            
            if (CommonTypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                if (world.stashes.TryGetValue(definition.id, out var index)) {
                    return Stash.stashes.data[index];
                }
            }

            var constructedType = typeof(Stash<>).MakeGenericType(type);
            var stash           = (Stash)Activator.CreateInstance(constructedType, true);

            stash.world = world;

            CommonTypeIdentifier.typeAssociation.TryGetValue(type, out definition);
            world.stashes.Add(definition.id, stash.commonStashId, out _);
            world.typedStashes.Add(definition.id, stash.typedStashId, out _);

            return stash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stash<T> GetStash<T>(this World world) where T : struct, IComponent {
            world.ThreadSafetyCheck();
            
            var info = TypeIdentifier<T>.info;
            if (world.typedStashes.TryGetValue(info.id, out var typedIndex)) {
                return Stash<T>.typedStashes.data[typedIndex];
            }

            var stash = new Stash<T>();
            stash.world = world;

            world.stashes.Add(info.id, stash.commonStashId, out _);
            world.typedStashes.Add(info.id, stash.typedStashId, out _);

            return stash;
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
            world.ThreadSafetyCheck();
            
            var newSysGroup = world.newSystemsGroups;

            for (var i = 0; i < newSysGroup.Count; i++) {
                var key          = newSysGroup.Keys[i];
                var systemsGroup = newSysGroup.Values[i];

                systemsGroup.Initialize();
                world.systemsGroups.Add(key, systemsGroup);
            }

            newSysGroup.Clear();

            for (var i = 0; i < world.newPluginSystemsGroups.length; i++) {
                var systemsGroup = world.newPluginSystemsGroups.data[i];

                systemsGroup.Initialize();
                world.pluginSystemsGroups.Add(systemsGroup);
            }
            
            world.newPluginSystemsGroups.Clear();

            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.Update(deltaTime);
            }
            for (var i = 0; i < world.pluginSystemsGroups.length; i++) {
                var systemsGroup = world.pluginSystemsGroups.data[i];
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
            world.ThreadSafetyCheck();
            
            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.FixedUpdate(deltaTime);
            }
            for (var i = 0; i < world.pluginSystemsGroups.length; i++) {
                var systemsGroup = world.pluginSystemsGroups.data[i];
                systemsGroup.FixedUpdate(deltaTime);
            }
        }

        public static void GlobalLateUpdate(float deltaTime) {
            foreach (var world in World.worlds) {
                if (world.UpdateByUnity) {
                    world.LateUpdate(deltaTime);
                    world.CleanupUpdate(deltaTime);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LateUpdate(this World world, float deltaTime) {
            world.ThreadSafetyCheck();
            
            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.LateUpdate(deltaTime);
            }
            for (var i = 0; i < world.pluginSystemsGroups.length; i++) {
                var systemsGroup = world.pluginSystemsGroups.data[i];
                systemsGroup.LateUpdate(deltaTime);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CleanupUpdate(this World world, float deltaTime) {
            world.ThreadSafetyCheck();
            
            for (var i = 0; i < world.systemsGroups.Count; i++) {
                var systemsGroup = world.systemsGroups.Values[i];
                systemsGroup.CleanupUpdate(deltaTime);
            }
            for (var i = 0; i < world.pluginSystemsGroups.length; i++) {
                var systemsGroup = world.pluginSystemsGroups.data[i];
                systemsGroup.CleanupUpdate(deltaTime);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SystemsGroup CreateSystemsGroup(this World world) {
            world.ThreadSafetyCheck();
            return new SystemsGroup(world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSystemsGroup(this World world, int order, SystemsGroup systemsGroup) {
            world.ThreadSafetyCheck();
            
            world.newSystemsGroups.Add(order, systemsGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPluginSystemsGroup(this World world, SystemsGroup systemsGroup) {
            world.ThreadSafetyCheck();
            
            world.newPluginSystemsGroups.Add(systemsGroup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSystemsGroup(this World world, SystemsGroup systemsGroup) {
            world.ThreadSafetyCheck();
            
            systemsGroup.Dispose();
            if (world.systemsGroups.ContainsValue(systemsGroup)) {
                world.systemsGroups.RemoveAt(world.systemsGroups.IndexOfValue(systemsGroup));
            }
            else if (world.newSystemsGroups.ContainsValue(systemsGroup)) {
                world.newSystemsGroups.RemoveAt(world.newSystemsGroups.IndexOfValue(systemsGroup));
            }
        }

        public static Entity CreateEntity(this World world) {
            world.ThreadSafetyCheck();
            
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
                Array.Resize(ref world.entitiesGens, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, World.worlds.IndexOf(world));
            ++world.entitiesCount;

            return world.entities[id];
        }

        public static Entity CreateEntity(this World world, out int id) {
            world.ThreadSafetyCheck();

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
                Array.Resize(ref world.entitiesGens, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, World.worlds.IndexOf(world));
            ++world.entitiesCount;

            return world.entities[id];
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this World world, in int id) {
            world.ThreadSafetyCheck();
            
            return world.entities[id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetEntity(this World world, in EntityId entityId, out Entity entity) {
            world.ThreadSafetyCheck();
            
            entity = default;

            if (entityId.id < 0 || entityId.id >= world.entitiesCapacity) {
                return false;
            }

            if (world.entitiesGens[entityId.id] != entityId.gen) {
                return false;
            }

            entity = world.entities[entityId.id];
            return !entity.IsNullOrDisposed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntity(this World world, Entity entity) {
            world.ThreadSafetyCheck();
            
            if (world.entities[entity.entityId.id] == entity) {
                entity.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyRemoveEntity(this World world, int id) {
            world.nextFreeEntityIDs.Add(id);
            world.entities[id] = null;
            world.entitiesGens[id]++;
            --world.entitiesCount;
        }

        public static void Commit(this World world) {
            world.ThreadSafetyCheck();
            
            MLogger.BeginSample("World.Commit()");
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

            if (world.nextFreeEntityIDs.length > 0) {
                world.freeEntityIDs.AddListRange(world.nextFreeEntityIDs);
                world.nextFreeEntityIDs.Clear();
            }
            MLogger.EndSample();
        }

        public static void SetFriendlyName(this World world, string friendlyName) {
            world.ThreadSafetyCheck();
            
            world.friendlyName = friendlyName;
        }

        public static string GetFriendlyName(this World world) {
            world.ThreadSafetyCheck();
            
            if (string.IsNullOrEmpty(world.friendlyName)) {
                return world.ToString() + world.identifier;
            }

            return world.friendlyName;
        }

        public static void SetThreadId(this World world, int threadId) {
            world.threadIdLock = threadId;
        }
        
        public static int GetThreadId(this World world) {
            return world.threadIdLock;
        }

        [System.Diagnostics.Conditional("MORPEH_THREAD_SAFETY")]
        internal static void ThreadSafetyCheck(this World world) {
            if (world == null) {
                return;
            }
            
            var currentThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (world.threadIdLock != currentThread) {
                throw new Exception($"[MORPEH] Thread safety check failed. You are trying touch the world from a thread {currentThread}, but the world associated with the thread {world.threadIdLock}");
            }
        }
    }
}
