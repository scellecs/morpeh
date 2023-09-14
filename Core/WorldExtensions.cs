#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
#if MORPEH_BURST
    using Unity.Collections;
#endif
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class WorldExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Ctor(this World world) {
            world.threadIdLock = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            world.systemsGroups    = new SortedList<int, SystemsGroup>();
            world.newSystemsGroups = new SortedList<int, SystemsGroup>();

            world.pluginSystemsGroups    = new FastList<SystemsGroup>();
            world.newPluginSystemsGroups = new FastList<SystemsGroup>();

            world.Filter           = new FilterBuilder{ world = world };
            world.filters          = new FastList<Filter>();
            world.filtersTree      = new LongHashMap<FilterNode>();
            world.filtersLookup    = new LongHashMap<LongHashMap<Filter>>();
            world.dirtyEntities    = new BitMap();
            
#if MORPEH_BURST
            world.tempArrays = new FastList<NativeArray<int>>();
#endif
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
            world.freeEntityIDs     = new IntStack();
            world.nextFreeEntityIDs = new IntStack();
            world.stashes           = new LongHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);
            world.typedStashes      = new LongHashMap<int>(Constants.DEFAULT_WORLD_CACHES_CAPACITY);

            world.entitiesCount    = 0;
            world.entitiesLength   = 0;
            world.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            world.entities         = new Entity[world.entitiesCapacity];
            world.entitiesGens     = new int[world.entitiesCapacity];

            world.archetypes         = new LongHashMap<Archetype>();
            world.archetypesCount    = 1;
            world.emptyArchetypes   = new FastList<Archetype>();
            world.removedArchetypes = new FastList<Archetype>();

            if (World.plugins != null) {
                foreach (var plugin in World.plugins) {
#if MORPEH_DEBUG
                    try {
#endif
                        plugin.Initialize(world);
#if MORPEH_DEBUG
                    }
                    catch (Exception e) {
                        MLogger.LogError($"Can not initialize world plugin {plugin.GetType()}");
                        MLogger.LogException(e);
                    }
#endif
                }
            }

            return world;
        }

#if MORPEH_UNITY && !MORPEH_DISABLE_AUTOINITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        [PublicAPI]
        public static void InitializationDefaultWorld() {
            Stash.cleanup();

            World.worlds.Clear();
            var defaultWorld = World.Create("Default World");
            defaultWorld.UpdateByUnity = true;
#if MORPEH_UNITY
            var go = new GameObject {
                name      = "MORPEH_UNITY_RUNTIME_HELPER",
                hideFlags = HideFlags.DontSaveInEditor
            };
            go.AddComponent<UnityRuntimeHelper>();
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddWorldPlugin<T>(T plugin) where T : class, IWorldPlugin {
            if (World.plugins == null) {
                World.plugins = new FastList<IWorldPlugin>();
            }
            World.plugins.Add(plugin);
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Stash GetStash(this World world, long typeId) {
            if (world.stashes.TryGetValue(typeId, out var index)) {
                return Stash.stashes.data[index];
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
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
        [PublicAPI]
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
        [PublicAPI]
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

        [PublicAPI]
        public static void GlobalFixedUpdate(float deltaTime) {
            foreach (var world in World.worlds) {
                if (world.UpdateByUnity) {
                    world.FixedUpdate(deltaTime);
                }
            }
        }

        [PublicAPI]
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

        [PublicAPI]
        public static void GlobalLateUpdate(float deltaTime) {
            foreach (var world in World.worlds) {
                if (world.UpdateByUnity) {
                    world.LateUpdate(deltaTime);
                    world.CleanupUpdate(deltaTime);
                }
            }
        }

        [PublicAPI]
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

        [PublicAPI]
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

            ref var m = ref world.newMetrics;
            m.entities = world.entitiesCount;
            m.archetypes = world.archetypes.length;
            m.filters = world.filters.length;
            foreach (var systemsGroup in world.systemsGroups.Values) {
                m.systems += systemsGroup.systems.length;
                m.systems += systemsGroup.fixedSystems.length;
                m.systems += systemsGroup.lateSystems.length;
                m.systems += systemsGroup.cleanupSystems.length;
            }
            world.metrics = m;
            m = default;
            
            
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void JobsComplete(this World world) {
            world.ThreadSafetyCheck();
#if MORPEH_BURST
            world.JobHandle.Complete();
            foreach (var array in world.tempArrays) {
                array.Dispose();
            }
            world.tempArrays.Clear();
#endif
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SystemsGroup CreateSystemsGroup(this World world) {
            world.ThreadSafetyCheck();
            return new SystemsGroup(world);
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSystemsGroup(this World world, int order, SystemsGroup systemsGroup) {
            world.ThreadSafetyCheck();
            
            world.newSystemsGroups.Add(order, systemsGroup);
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPluginSystemsGroup(this World world, SystemsGroup systemsGroup) {
            world.ThreadSafetyCheck();
            
            world.newPluginSystemsGroups.Add(systemsGroup);
        }

        [PublicAPI]
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

        [PublicAPI]
        public static Entity CreateEntity(this World world) {
            world.ThreadSafetyCheck();
            
            int id;
            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Pop();
            }
            else {
                id = world.entitiesLength++;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                var newCapacity = HashHelpers.GetCapacity(world.entitiesCapacity) + 1;
                Array.Resize(ref world.entities, newCapacity);
                Array.Resize(ref world.entitiesGens, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, world);
            ++world.entitiesCount;

            return world.entities[id];
        }

        [PublicAPI]
        public static Entity CreateEntity(this World world, out int id) {
            world.ThreadSafetyCheck();

            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Pop();
            }
            else {
                id = world.entitiesLength++;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                var newCapacity = HashHelpers.GetCapacity(world.entitiesCapacity) + 1;
                Array.Resize(ref world.entities, newCapacity);
                Array.Resize(ref world.entitiesGens, newCapacity);
                world.entitiesCapacity = newCapacity;
            }

            world.entities[id] = EntityExtensions.Create(id, world);
            ++world.entitiesCount;

            return world.entities[id];
        }

        [CanBeNull]
        [PublicAPI]
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

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntity(this World world, Entity entity) {
            world.ThreadSafetyCheck();
            
            if (world.entities[entity.entityId.id] == entity) {
                entity.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyRemoveEntity(this World world, int id) {
            world.nextFreeEntityIDs.Push(id);
            world.entities[id] = null;
            world.entitiesGens[id]++;
            --world.entitiesCount;
        }

        [PublicAPI]
        public static void Commit(this World world) {
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.iteratorLevel > 0) {
                MLogger.LogError("You can not call world.Commit() inside Filter foreach loop. Place it outside of foreach block. ");
                return;
            }
#endif
            
            world.newMetrics.commits++;
            MLogger.BeginSample("World.Commit()");
#if MORPEH_DEBUG && MORPEH_BURST
            if (world.dirtyEntities.count > 0 && (world.JobHandle.IsCompleted == false)) {
                MLogger.LogError("You have changed entities before all scheduled jobs are completed. This may lead to unexpected behavior or crash. Jobs will be forced.");
                world.JobsComplete();
            }
#endif
            world.newMetrics.migrations += world.dirtyEntities.count;
            
            foreach (var entityId in world.dirtyEntities) {
                world.entities[entityId]?.ApplyTransfer();
            }

            world.dirtyEntities.Clear();
            
            if (world.removedArchetypes.length > 0) {
                for (var index = 0; index < world.removedArchetypes.length; index++) {
                    var arch = world.removedArchetypes.data[index];
                    for (var i = 0; i < arch.filters.length; i++) {
                        var filter = arch.filters.data[i];
                        filter.RemoveArchetype(arch);
                        arch.filters.data[i] = default;
                    }
                    arch.filters.length = 0;
                    arch.filters.lastSwappedIndex = -1;
                }

                world.emptyArchetypes.AddListRange(world.removedArchetypes);
                world.removedArchetypes.Clear();
            }

            if (world.nextFreeEntityIDs.length > 0) {
                world.freeEntityIDs.PushRange(world.nextFreeEntityIDs);
                world.nextFreeEntityIDs.Clear();
            }
            MLogger.EndSample();
        }

        [PublicAPI]
        public static void SetFriendlyName(this World world, string friendlyName) {
            world.ThreadSafetyCheck();
            
            world.friendlyName = friendlyName;
        }

        [PublicAPI]
        public static string GetFriendlyName(this World world) {
            world.ThreadSafetyCheck();
            
            if (string.IsNullOrEmpty(world.friendlyName)) {
                return world.ToString() + world.identifier;
            }

            return world.friendlyName;
        }

        [PublicAPI]
        public static void SetThreadId(this World world, int threadId) {
            world.ThreadSafetyCheck();
            world.threadIdLock = threadId;
        }
        
        [PublicAPI]
        public static int GetThreadId(this World world) {
            return world.threadIdLock;
        }

        [System.Diagnostics.Conditional("MORPEH_THREAD_SAFETY")]
        internal static void ThreadSafetyCheck(this World world) {
            if (world == null) {
                return;
            }

            var currentThread = Environment.CurrentManagedThreadId;
            if (world.threadIdLock != currentThread) {
                throw new Exception($"[MORPEH] Thread safety check failed. You are trying touch the world from a thread {currentThread}, but the world associated with the thread {world.threadIdLock}");
            }
        }
        
        [PublicAPI]
        public static void WarmupArchetypes(this World world, int count) {
            for (int i = 0, length = count; i < length; i++) {
                world.emptyArchetypes.Add(new Archetype(-1, world));
            }
        }
        
        [PublicAPI]
        public static AspectFactory<T> GetAspectFactory<T>(this World world) where T : struct, IAspect {
            world.ThreadSafetyCheck();
            var aspectFactory = default(AspectFactory<T>);
            aspectFactory.value.OnGetAspectFactory(world);
            return aspectFactory;
        }
        
        [PublicAPI]
        public static bool IsNullOrDisposed(this World world) {
            return world == null || world.IsDisposed;
        }
    }
}
