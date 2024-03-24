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
            world.disposedEntities = new BitMap();
            
#if MORPEH_BURST
            world.tempArrays = new FastList<NativeArray<Entity>>();
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
            world.stashes           = new Stash[Constants.DEFAULT_WORLD_STASHES_CAPACITY];

            world.entitiesCount    = 0;
            world.entitiesLength   = 0;
            world.entitiesCapacity = Constants.DEFAULT_WORLD_ENTITIES_CAPACITY;
            
            world.entities = new EntityData[world.entitiesCapacity];
            for (var i = 0; i < world.entitiesCapacity; i++) {
                world.entities[i].Initialize();
            }
            world.entitiesGens = new int[world.entitiesCapacity];

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
            var defaultWorld = World.Create();
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
                id = ++world.entitiesLength;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                world.ExpandEntities();
            }

            ++world.entitiesCount;
            return new Entity(world.identifier, id, world.entitiesGens[id]);
        }

        [PublicAPI]
        public static Entity CreateEntity(this World world, out int id) {
            world.ThreadSafetyCheck();

            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Pop();
            }
            else {
                id = ++world.entitiesLength;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                world.ExpandEntities();
            }
            
            ++world.entitiesCount;
            return new Entity(world.identifier, id, world.entitiesGens[id]);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ExpandEntities(this World world) {
            var oldCapacity = world.entitiesCapacity;
            var newCapacity = HashHelpers.GetCapacity(world.entitiesCapacity) + 1;
            
            Array.Resize(ref world.entities, newCapacity);
            for (var i = oldCapacity; i < newCapacity; i++)
            {
                world.entities[i].Initialize();
            }
            
            Array.Resize(ref world.entitiesGens, newCapacity);

            world.entitiesCapacity = newCapacity;
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntityAtIndex(this World world, int id) {
            world.ThreadSafetyCheck();
            return new Entity(world.identifier, id, world.entitiesGens[id]);
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntity(this World world, Entity entity) {
            if (world.IsDisposed(entity)) {
#if MORPEH_DEBUG
                MLogger.LogError($"You're trying to dispose disposed entity {entity}.");
#endif
                return;
            }
            
            ref var entityData = ref world.entities[entity.Id];
            
            // Clear new components if entity is transient
            
            if (world.dirtyEntities.Get(entity.Id)) {
                // As we clean stashes, changes may be modified, so we need to copy them to stack
                
                Span<StructuralChange> changes = stackalloc StructuralChange[entityData.changesCount];
                for (var i = 0; i < entityData.changesCount; i++) {
                    changes[i] = entityData.changes[i];
                }
                
                var changesCount = changes.Length;
                
                for (var i = 0; i < changesCount; i++) {
                    var structuralChange = changes[i];

                    if (!structuralChange.isAddition) {
                        continue;
                    }
                    
                    world.GetStash(structuralChange.typeOffset.GetValue())?.Remove(entity);
                }
            }
            
            // Clear components from existing archetype
            
            if (entityData.currentArchetype != null) {
                foreach (var offset in entityData.currentArchetype.components) {
                    world.GetStash(offset)?.Remove(entity);
                }
            }
            
            world.dirtyEntities.Unset(entity.Id);
            world.disposedEntities.Set(entity.Id);
            
            world.IncrementGeneration(entity.Id);
            --world.entitiesCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed(this World world, Entity entity) {
            return entity == default || world.entitiesGens[entity.Id] != entity.Generation;
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
            if (world.disposedEntities.count > 0) {
                world.CompleteDisposals();
            }
            
            if (world.dirtyEntities.count > 0) {
                world.newMetrics.migrations += world.dirtyEntities.count;
                world.ApplyTransientChanges();
            }
            
            if (world.nextFreeEntityIDs.length > 0) {
                world.PushFreeIds();
            }

            if (world.emptyArchetypes.length > 0) {
                world.ClearEmptyArchetypes();
            }

            MLogger.EndSample();
            
            MLogger.LogTrace("[WorldExtensions] Commit done");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void CompleteDisposals(this World world) {
            foreach (var entityId in world.disposedEntities) {
                world.CompleteEntityDisposal(entityId, ref world.entities[entityId]);
            }
            
            world.disposedEntities.Clear();
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ApplyTransientChanges(this World world) {
            var clearedEntities = 0;
            
            foreach (var entityId in world.dirtyEntities) {
                ref var entityData = ref world.entities[entityId];
                
                if (entityData.nextArchetypeId == ArchetypeId.Invalid) {
                    world.CompleteEntityDisposal(entityId, ref entityData);
                    world.IncrementGeneration(entityId);
                    clearedEntities++;
                } else {
                    world.ApplyTransientChanges(new Entity(world.identifier, entityId, world.entitiesGens[entityId]), ref entityData);
                }
            }

            world.entitiesCount -= clearedEntities;
            world.dirtyEntities.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void PushFreeIds(this World world) {
            world.freeEntityIDs.PushRange(world.nextFreeEntityIDs);
            world.nextFreeEntityIDs.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ClearEmptyArchetypes(this World world) {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeAddComponent(this World world, Entity entity, ref TypeInfo typeInfo) {
            ref var entityData = ref world.entities[entity.Id];
            
            if (world.dirtyEntities.Set(entity.Id)) {
                EntityDataUtility.Rebase(ref entityData);
            }
            
            EntityDataUtility.AddComponent(ref entityData, ref typeInfo);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeRemoveComponent(this World world, Entity entity, ref TypeInfo typeInfo) {
            ref var entityData = ref world.entities[entity.Id];
            
            if (world.dirtyEntities.Set(entity.Id)) {
                EntityDataUtility.Rebase(ref entityData);
            }
            
            EntityDataUtility.RemoveComponent(ref entityData, ref typeInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransientChanges(this World world, Entity entity, ref EntityData entityData) {
            var currentArchetypeId = entityData.currentArchetype?.id ?? ArchetypeId.Invalid;
            
            // No changes
            
            if (entityData.nextArchetypeId == currentArchetypeId) {
                MLogger.LogTrace($"[WorldExtensions] No changes for entity {entity}");
                return;
            }
            
            // Add to new archetype

            var indexInNextArchetype = 0;
            
            if (world.archetypes.TryGetValue(entityData.nextArchetypeId.GetValue(), out var nextArchetype)) {
                MLogger.LogTrace($"[WorldExtensions] Add entity {entity} to EXISTING archetype {nextArchetype.id}");
                indexInNextArchetype = nextArchetype.Add(entity);
            } else {
                MLogger.LogTrace($"[WorldExtensions] Add entity {entity} to NEW archetype {entityData.nextArchetypeId}");
                nextArchetype = world.CreateMigratedArchetype(ref entityData);
                indexInNextArchetype = nextArchetype.Add(entity);
                
                world.archetypes.Add(entityData.nextArchetypeId.GetValue(), nextArchetype, out _);
                world.archetypesCount++;

                if (currentArchetypeId != ArchetypeId.Invalid) {
                    AddMatchingPreviousFilters(nextArchetype, ref entityData);
                }
                
                world.AddMatchingDeltaFilters(nextArchetype, ref entityData);
                
                MLogger.LogTrace($"[WorldExtensions] Filter count for archetype {nextArchetype.id} is {nextArchetype.filters.length}");
            }
            
            // Remove from previous archetype
            
            if (currentArchetypeId != ArchetypeId.Invalid) {
                var index = entityData.indexInCurrentArchetype;
                entityData.currentArchetype.Remove(index);
                
                var entityIndex = entityData.currentArchetype.entities[index].Id;
                world.entities[entityIndex].indexInCurrentArchetype = index;
                
                world.TryScheduleArchetypeForRemoval(entityData.currentArchetype);
            }
            
            // Finalize migration
            MLogger.LogTrace($"[WorldExtensions] Finalize migration for entity {entity} to archetype {nextArchetype.id}");
            entityData.currentArchetype = nextArchetype;
            entityData.indexInCurrentArchetype = indexInNextArchetype;
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
        internal static Archetype CreateMigratedArchetype(this World world, ref EntityData entityData) {
            var nextArchetype = world.archetypePool.Rent(entityData.nextArchetypeId);
            
            if (entityData.currentArchetype != null) {
                MLogger.LogTrace($"[WorldExtensions] Copy {entityData.currentArchetype.components.count} components from base archetype {entityData.currentArchetype.id}");
                foreach (var offset in entityData.currentArchetype.components) {
                    MLogger.LogTrace($"[WorldExtensions] Copy component {offset} from base archetype {entityData.currentArchetype.id}");
                    nextArchetype.components.Set(offset);
                }
            } else {
                MLogger.LogTrace($"[WorldExtensions] Base archetype is null");
            }
            
            MLogger.LogTrace($"[WorldExtensions] Add {entityData.changesCount} components to archetype {entityData.nextArchetypeId}");
            var changesCount = entityData.changesCount;
            for (var i = 0; i < changesCount; i++) {
                ref var structuralChange = ref entityData.changes[i];
                if (structuralChange.isAddition) {
                    MLogger.LogTrace($"[WorldExtensions] Add {structuralChange.typeOffset} to archetype {entityData.nextArchetypeId}");
                    nextArchetype.components.Set(structuralChange.typeOffset.GetValue());
                } else {
                    MLogger.LogTrace($"[WorldExtensions] Remove {structuralChange.typeOffset} from archetype {entityData.nextArchetypeId}");
                    nextArchetype.components.Unset(structuralChange.typeOffset.GetValue());
                }
            }
            
            return nextArchetype;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddMatchingPreviousFilters(Archetype archetype, ref EntityData transient) {
            foreach (var idx in transient.currentArchetype.filters) {
                var filter = transient.currentArchetype.filters.GetValueByIndex(idx);
                if (filter.AddArchetypeIfMatches(archetype)) {
                    MLogger.LogTrace($"[WorldExtensions] Add PREVIOUS {filter} to archetype {archetype.id}");
                    archetype.AddFilter(filter);
                } else {
                    MLogger.LogTrace($"[WorldExtensions] PREVIOUS {filter} does not match archetype {archetype.id}");
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddMatchingDeltaFilters(this World world, Archetype archetype, ref EntityData transient) {
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CompleteEntityDisposal(this World world, int entityId, ref EntityData entityData) {
            if (entityData.currentArchetype != null) {
                var index = entityData.indexInCurrentArchetype;
                entityData.currentArchetype.Remove(index);
                
                var entityIndex = entityData.currentArchetype.entities[index].Id;
                world.entities[entityIndex].indexInCurrentArchetype = index;
                
                world.TryScheduleArchetypeForRemoval(entityData.currentArchetype);
                
                entityData.currentArchetype = null;
            }
            
            world.nextFreeEntityIDs.Push(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IncrementGeneration(this World world, int entityId) {
            // Gens can only be 3 bytes long (0xFFFFFF)
            
            if (++world.entitiesGens[entityId] >= 0xFFFFFF) {
                world.entitiesGens[entityId] = 0;
            }
        }
        
        [PublicAPI]
        public static void WarmupArchetypes(this World world, int count) {
            world.ThreadSafetyCheck();
            
            world.archetypePool.WarmUp(count);
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
