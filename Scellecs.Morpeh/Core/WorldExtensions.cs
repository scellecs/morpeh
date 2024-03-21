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
            world.stashes           = new IntHashMap<Stash>(Constants.DEFAULT_WORLD_STASHES_CAPACITY);

            world.entitiesCount    = 0;
            world.entitiesLength   = 0;
            world.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            world.entities         = new Entity[world.entitiesCapacity];
            world.entitiesGens     = new int[world.entitiesCapacity];
            
            world.transients       = new TransientArchetype[world.entitiesCapacity];
            for (var i = 0; i < world.entitiesCapacity; i++) {
                TransientUtility.Initialize(ref world.transients[i]);
            }

            world.archetypes         = new LongHashMap<Archetype>();
            world.archetypesCount    = 1;
            
            world.archetypePool = new ArchetypePool(32);
            world.emptyArchetypes = new FastList<Archetype>();

            world.componentsToFiltersRelation = new ComponentsToFiltersRelation(256);

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
            foreach (var world in World.worlds) {
                if (!world.IsNullOrDisposed()) {
                    world.Dispose();
                }
            }
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
        internal static Stash GetStash(this World world, TypeOffset offset) {
            return world.stashes.TryGetValue(offset.GetValue(), out var value) ? value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static Stash GetReflectionStash(this World world, Type type) {
            world.ThreadSafetyCheck();
            
            if (TypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                if (world.stashes.TryGetValue(definition.offset.GetValue(), out var value)) {
                    return value;
                }
            }

            var stash = Stash.CreateReflection(world, type);
            TypeIdentifier.typeAssociation.TryGetValue(type, out definition);
            world.stashes.Add(definition.offset.GetValue(), stash, out _);

            return stash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static Stash<T> GetStash<T>(this World world) where T : struct, IComponent {
            world.ThreadSafetyCheck();
            
            var info = TypeIdentifier<T>.info;
            if (world.stashes.TryGetValue(info.offset.GetValue(), out var value)) {
                return (Stash<T>)value.typelessStash;
            }

            var stash = Stash.Create<T>(world);
            world.stashes.Add(info.offset.GetValue(), stash, out _);

            return (Stash<T>)stash.typelessStash;
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
            for (int index = 0, length = world.systemsGroups.Values.Count; index < length; index++) {
                var systemsGroup = world.systemsGroups.Values[index];
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
                var oldCapacity = world.entitiesCapacity;
                var newCapacity = HashHelpers.GetCapacity(world.entitiesCapacity) + 1;
                Array.Resize(ref world.entities, newCapacity);
                Array.Resize(ref world.entitiesGens, newCapacity);
                Array.Resize(ref world.transients, newCapacity);
                for (var i = oldCapacity; i < newCapacity; i++) {
                    TransientUtility.Initialize(ref world.transients[i]);
                }
                
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
                var oldCapacity = world.entitiesCapacity;
                var newCapacity = HashHelpers.GetCapacity(world.entitiesCapacity) + 1;
                Array.Resize(ref world.entities, newCapacity);
                Array.Resize(ref world.entitiesGens, newCapacity);
                Array.Resize(ref world.transients, newCapacity);
                for (var i = oldCapacity; i < newCapacity; i++) {
                    TransientUtility.Initialize(ref world.transients[i]);
                }
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
            MLogger.LogTrace("[WorldExtensions] Commit");
            
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
                var entity = world.entities[entityId];
                world.ApplyTransientChanges(entity);
            }

            world.dirtyEntities.Clear();

            if (world.nextFreeEntityIDs.length > 0) {
                world.freeEntityIDs.PushRange(world.nextFreeEntityIDs);
                world.nextFreeEntityIDs.Clear();
            }
            
            if (world.emptyArchetypes.length > 0) {
                foreach (var archetype in world.emptyArchetypes) {
                    if (!archetype.IsEmpty()) {
                        MLogger.LogTrace($"[WorldExtensions] Archetype {archetype.id} is not empty after complete migration of entities");
                        continue;
                    }
                    
                    MLogger.LogTrace($"[WorldExtensions] Remove archetype {archetype.id}");
                    
                    world.archetypes.Remove(archetype.id.GetValue(), out _);
                    world.archetypesCount--;
                    
                    world.archetypePool.Return(archetype);
                }
                
                world.emptyArchetypes.Clear();
            }
            MLogger.EndSample();
            
            MLogger.LogTrace("[WorldExtensions] Commit done");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeAddComponent(this World world, Entity entity, ref TypeInfo typeInfo) {
            ref var transient = ref world.transients[entity.entityId.id];
            
            if (world.dirtyEntities.Set(entity.entityId.id)) {
                TransientUtility.Rebase(ref transient, world.archetypes.GetValueByKey(entity.currentArchetype.GetValue()));
            }
            
            TransientUtility.AddComponent(ref transient, ref typeInfo);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeRemoveComponent(this World world, Entity entity, ref TypeInfo typeInfo) {
            ref var transient = ref world.transients[entity.entityId.id];
            
            if (world.dirtyEntities.Set(entity.entityId.id)) {
                TransientUtility.Rebase(ref transient, world.archetypes.GetValueByKey(entity.currentArchetype.GetValue()));
            }
            
            TransientUtility.RemoveComponent(ref transient, ref typeInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransientChanges(this World world, Entity entity) {
            ref var transient = ref world.transients[entity.entityId.id];

            // No changes
            
            if (transient.nextArchetypeId == entity.currentArchetype) {
                MLogger.LogTrace($"[WorldExtensions] No changes for entity {entity}");
                return;
            }
            
            // Destroy entity if all components have been removed
            
            if (transient.nextArchetypeId == ArchetypeId.Invalid) {
                MLogger.LogTrace($"[WorldExtensions] Destroying entity {entity} because all components have been removed");
                entity.Dispose();
                return;
            }
            
            // Add to new archetype
            
            if (world.archetypes.TryGetValue(transient.nextArchetypeId.GetValue(), out var nextArchetype)) {
                MLogger.LogTrace($"[WorldExtensions] Add entity {entity} to EXISTING archetype {nextArchetype.id}");
                nextArchetype.Add(entity.ID);
            } else {
                MLogger.LogTrace($"[WorldExtensions] Add entity {entity} to NEW archetype {transient.nextArchetypeId}");
                nextArchetype = world.CreateArchetypeFromTransient(ref transient);
                nextArchetype.Add(entity.ID);
                
                world.archetypes.Add(transient.nextArchetypeId.GetValue(), nextArchetype, out _);
                world.archetypesCount++;

                if (transient.baseArchetype != null) {
                    AddMatchingPreviousFilters(nextArchetype, ref transient);
                }
                
                world.AddMatchingDeltaFilters(nextArchetype, ref transient);
                
                MLogger.LogTrace($"[WorldExtensions] Filter count for archetype {nextArchetype.id} is {nextArchetype.filters.length}");
            }
            
            // Remove from previous archetype
            
            if (transient.baseArchetype != null) {
                transient.baseArchetype.Remove(entity.ID);
                world.TryScheduleArchetypeForRemoval(transient.baseArchetype);
            }
            
            // Finalize migration
            MLogger.LogTrace($"[WorldExtensions] Finalize migration for entity {entity} to archetype {nextArchetype.id}");
            entity.currentArchetype = transient.nextArchetypeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TryScheduleArchetypeForRemoval(this World world, Archetype archetype) {
            if (!archetype.IsEmpty()) {
                return;
            }
            
            MLogger.LogTrace($"[WorldExtensions] Schedule archetype {archetype.id} for removal");
            world.emptyArchetypes.Add(archetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Archetype CreateArchetypeFromTransient(this World world, ref TransientArchetype transient) {
            var nextArchetype = world.archetypePool.Rent(transient.nextArchetypeId);
            
            if (transient.baseArchetype != null) {
                MLogger.LogTrace($"[WorldExtensions] Copy {transient.baseArchetype.components.count} components from base archetype {transient.baseArchetype.id}");
                foreach (var offset in transient.baseArchetype.components) {
                    MLogger.LogTrace($"[WorldExtensions] Copy component {offset} from base archetype {transient.baseArchetype.id}");
                    nextArchetype.components.Set(offset);
                }
            } else {
                MLogger.LogTrace($"[WorldExtensions] Base archetype is null");
            }
            
            MLogger.LogTrace($"[WorldExtensions] Add {transient.changesCount} components to archetype {transient.nextArchetypeId}");
            var changesCount = transient.changesCount;
            for (var i = 0; i < changesCount; i++) {
                ref var structuralChange = ref transient.changes[i];
                if (structuralChange.isAddition) {
                    MLogger.LogTrace($"[WorldExtensions] Add {structuralChange.typeOffset} to archetype {transient.nextArchetypeId}");
                    nextArchetype.components.Set(structuralChange.typeOffset.GetValue());
                } else {
                    MLogger.LogTrace($"[WorldExtensions] Remove {structuralChange.typeOffset} from archetype {transient.nextArchetypeId}");
                    nextArchetype.components.Unset(structuralChange.typeOffset.GetValue());
                }
            }
            
            return nextArchetype;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddMatchingPreviousFilters(Archetype archetype, ref TransientArchetype transient) {
            foreach (var idx in transient.baseArchetype.filters) {
                var filter = transient.baseArchetype.filters.GetValueByIndex(idx);
                if (filter.AddArchetypeIfMatches(archetype)) {
                    MLogger.LogTrace($"[WorldExtensions] Add PREVIOUS {filter} to archetype {archetype.id}");
                    archetype.AddFilter(filter);
                } else {
                    MLogger.LogTrace($"[WorldExtensions] PREVIOUS {filter} does not match archetype {archetype.id}");
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddMatchingDeltaFilters(this World world, Archetype archetype, ref TransientArchetype transient) {
            var changesCount = transient.changesCount;
            for (var i = 0; i < changesCount; i++) {
                ref var structuralChange = ref transient.changes[i];
                
                var filters = world.componentsToFiltersRelation.GetFilters(structuralChange.typeOffset.GetValue());
                if (filters == null) {
                    MLogger.LogTrace($"[WorldExtensions] No DELTA filters for component {structuralChange.typeOffset.GetValue()}");
                    continue;
                }
                
                MLogger.LogTrace($"[WorldExtensions] Found {filters.Length} DELTA filters for component {structuralChange.typeOffset.GetValue()}");
                
                var filtersCount = filters.Length;
                for (var j = 0; j < filtersCount; j++) {
                    var filter = filters[j];
                    
                    if (filter.AddArchetypeIfMatches(archetype)) {
                        MLogger.LogTrace($"[WorldExtensions] Add DELTA filter {filter} to archetype {archetype.id}");
                        archetype.AddFilter(filter);
                    } else {
                        MLogger.LogTrace($"[WorldExtensions] DELTA filter {filter} does not match archetype {archetype.id}");
                    }
                }
            }
        }
        
        [PublicAPI]
        public static void WarmupArchetypes(this World world, int count) {
            world.ThreadSafetyCheck();
            
            world.archetypePool.WarmUp(count);
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
