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
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class WorldExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static World Initialize(this World world) {
            var id = -1;
            for (int i = 0; i < WorldConstants.MAX_WORLDS_COUNT; i++) {
                if (World.worldsIndices[i] == 0) {
                    id = i;
                    break;
                }
            }

            world.identifier        = id;
            world.generation        = World.worldsGens[id];
            world.freeEntityIDs     = new IntStack();
            world.stashes           = new IStash[WorldConstants.DEFAULT_STASHES_CAPACITY];

            world.entitiesCount    = 0;
            world.entitiesLength   = 0;
            world.entitiesCapacity = WorldConstants.DEFAULT_ENTITIES_CAPACITY;
            
            world.entities = new EntityData[world.entitiesCapacity];
            for (var i = 0; i < world.entitiesCapacity; i++) {
                world.entities[i].Initialize();
            }
            world.entitiesGens = new ushort[world.entitiesCapacity];
            world.dirtyEntities    = new IntSparseSet(world.entitiesCapacity);
            world.disposedEntities = new IntSparseSet(world.entitiesCapacity);

            world.archetypes      = new ArchetypeStore();
            world.archetypesCount = 0;
            
            world.archetypePool        = new ArchetypePool(32);
            world.emptyArchetypes      = new Archetype[32];
            world.emptyArchetypesCount = 0;

            world.componentsFiltersWith = new ComponentsToFiltersRelation(128);
            world.componentsFiltersWithout = new ComponentsToFiltersRelation(128);

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

            world.ApplyAddWorld();
            return world;
        }

        internal static void ApplyAddWorld(this World world) {
            World.worlds[World.worldsCount] = world;
            World.worldsIndices[world.identifier] = World.worldsCount + 1;
            World.defaultWorld = World.worlds[0];
            World.worldsCount++;
        }

        internal static void ApplyRemoveWorld(this World world) {
            unchecked {
                World.worldsGens[world.identifier]++;
            }

            var currentIndex = World.worldsIndices[world.identifier] - 1;
            World.worldsCount--;

            if (currentIndex < World.worldsCount) {
                var swapWorld = World.worlds[World.worldsCount];
                World.worlds[currentIndex] = swapWorld;
                World.worldsIndices[swapWorld.identifier] = currentIndex + 1;
            }

            World.worlds[World.worldsCount] = null;
            World.worldsIndices[world.identifier] = 0;
            World.defaultWorld = World.worldsCount > 0 ? World.worlds[0] : null;
            world.identifier = -1;
            world.generation = -1;
        }

#if MORPEH_UNITY && !MORPEH_DISABLE_AUTOINITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        [PublicAPI]
        public static void InitializationDefaultWorld() {
            World.DisposeAllWorlds();

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
        public static void Commit(this World world) {
            MLogger.LogTrace("[WorldExtensions] Commit");
            
            world.ThreadSafetyCheck();
            
#if MORPEH_DEBUG
            if (world.iteratorLevel > 0) {
                MLogger.LogError("You can not call world.Commit() inside Filter foreach loop. Place it outside of foreach block. ");
                return;
            }
            
            world.newMetrics.commits++;
#endif
            
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
#if MORPEH_DEBUG
                world.newMetrics.migrations += world.dirtyEntities.count;
#endif
                world.ApplyTransientChanges();
            }

            if (world.emptyArchetypesCount > 0) {
                world.ClearEmptyArchetypes();
            }

            MLogger.EndSample();
            
            MLogger.LogTrace("[WorldExtensions] Commit done");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void CompleteDisposals(this World world) {
            var sparseSet = world.disposedEntities;
            
            for (var i = sparseSet.count - 1; i >= 0; i--) {
                var entityId = sparseSet.dense[i];
                world.CompleteEntityDisposal(entityId, ref world.entities[entityId]);
                
                sparseSet.sparse[entityId] = 0;
            }
            
            sparseSet.count = 0;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ApplyTransientChanges(this World world) {
            var sparseSet = world.dirtyEntities;
            
            for (var i = sparseSet.count - 1; i >= 0; i--) {
                var entityId = sparseSet.dense[i];
                ref var entityData = ref world.entities[entityId];
                
                if (entityData.nextArchetypeHash == default) {
                    world.CompleteEntityDisposal(entityId, ref entityData);
                    world.IncrementGeneration(entityId);
                    --world.entitiesCount;
                } else if (entityData.addedComponentsCount + entityData.removedComponentsCount > 0) {
                    world.ApplyTransientChanges(entityId, ref entityData);
                }
                
                sparseSet.sparse[entityId] = 0;
            }

            sparseSet.count = 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ClearEmptyArchetypes(this World world) {
            var emptyArchetypes = world.emptyArchetypes;
            
            for (var i = world.emptyArchetypesCount - 1; i >= 0; i--) {
                var archetype = emptyArchetypes[i];
                
                if (!archetype.IsEmpty()) {
                    MLogger.LogTrace($"[WorldExtensions] Archetype {archetype.hash} is not empty after complete migration of entities");
                    continue;
                }
                
                MLogger.LogTrace($"[WorldExtensions] Remove archetype {archetype.hash}");
                
                for (int slotIndex = 0, lastIndex = archetype.filtersMap.lastIndex; slotIndex < lastIndex; slotIndex++) {
                    if (archetype.filtersMap.slots[slotIndex].key - 1 < 0) {
                        continue;
                    }
                    
                    archetype.filters[slotIndex].RemoveArchetype(archetype);
                }
                
                archetype.ClearFilters();
                archetype.components.Clear();
                
                world.archetypes.Remove(archetype);
                world.archetypesCount--;
                world.archetypePool.Return(archetype);

                emptyArchetypes[i] = null;
            }
            
            world.emptyArchetypesCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeAddComponent(this World world, int entityId, ref TypeInfo typeInfo) {
            ref var entityData = ref world.entities[entityId];
            entityData.nextArchetypeHash = entityData.nextArchetypeHash.Combine(typeInfo.hash);
            
            for (var i = entityData.removedComponentsCount - 1; i >= 0; i--) {
                if (entityData.removedComponents[i] != typeInfo.id) {
                    continue;
                }
                
                entityData.removedComponents[i] = entityData.removedComponents[--entityData.removedComponentsCount];
                return;
            }
            
            if (entityData.addedComponentsCount == entityData.addedComponents.Length) {
                ExpandAddedComponents(ref entityData);
            }

            entityData.addedComponents[entityData.addedComponentsCount++] = typeInfo.id;
            world.dirtyEntities.Add(entityId);
            
            MLogger.LogTrace($"[AddComponent] To: {entityData.nextArchetypeHash}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ExpandAddedComponents(ref EntityData entityData) {
            ArrayHelpers.Grow(ref entityData.addedComponents, entityData.addedComponentsCount << 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeRemoveComponent(this World world, int entityId, ref TypeInfo typeInfo) {
            ref var entityData = ref world.entities[entityId];
            entityData.nextArchetypeHash = entityData.nextArchetypeHash.Combine(typeInfo.hash);
            
            for (var i = entityData.addedComponentsCount - 1; i >= 0; i--) {
                if (entityData.addedComponents[i] != typeInfo.id) {
                    continue;
                }
                
                entityData.addedComponents[i] = entityData.addedComponents[--entityData.addedComponentsCount];
                return;
            }
            
            if (entityData.removedComponentsCount == entityData.removedComponents.Length) {
                ExpandRemovedComponents(ref entityData);
            }
            
            entityData.removedComponents[entityData.removedComponentsCount++] = typeInfo.id;
            world.dirtyEntities.Add(entityId);
            
            MLogger.LogTrace($"[RemoveComponent] To: {entityData.nextArchetypeHash}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ExpandRemovedComponents(ref EntityData entityData) {
            ArrayHelpers.Grow(ref entityData.removedComponents, entityData.removedComponentsCount << 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransientChanges(this World world, int entityId, ref EntityData entityData) {
            // Add to new archetype
            if (!world.archetypes.TryGet(entityData.nextArchetypeHash, out var nextArchetype)) {
                nextArchetype = world.CreateMigratedArchetype(ref entityData);
            }
            
            var indexInNextArchetype = nextArchetype.AddEntity(world.GetEntityAtIndex(entityId));
            
            // Remove from previous archetype
            if (entityData.currentArchetype != null) {
                var index = entityData.indexInCurrentArchetype;
                entityData.currentArchetype.RemoveEntityAtIndex(index);
                
                var entityIndex = entityData.currentArchetype.entities[index].Id;
                world.entities[entityIndex].indexInCurrentArchetype = index;
                
                world.TryScheduleArchetypeForRemoval(entityData.currentArchetype);
            }
            
            // Finalize migration
            entityData.currentArchetype = nextArchetype;
            entityData.indexInCurrentArchetype = indexInNextArchetype;
            
            entityData.addedComponentsCount = 0;
            entityData.removedComponentsCount = 0;
            
            entityData.nextArchetypeHash = nextArchetype.hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TryScheduleArchetypeForRemoval(this World world, Archetype archetype) {
            if (!archetype.IsEmpty()) {
                return;
            }
            
            MLogger.LogTrace($"[WorldExtensions] Schedule archetype {archetype.hash} for removal");
            if (world.emptyArchetypesCount == world.emptyArchetypes.Length) {
                world.GrowEmptyArchetypes();
            }
            
            world.emptyArchetypes[world.emptyArchetypesCount++] = archetype;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void GrowEmptyArchetypes(this World world) {
            ArrayHelpers.Grow(ref world.emptyArchetypes, world.emptyArchetypesCount << 1);
        }
        
        internal static Archetype CreateMigratedArchetype(this World world, ref EntityData entityData) {
            var nextArchetype = world.archetypePool.Rent(entityData.nextArchetypeHash);
            
            if (entityData.currentArchetype != null) {
                // These checks can happen before setting components to new archetype
                // because we only check for added/removed components

                var filtersMap = entityData.currentArchetype.filtersMap;
                var filters= entityData.currentArchetype.filters;
            
                for (int slotIndex = 0, lastIndex = filtersMap.lastIndex; slotIndex < lastIndex; slotIndex++) {
                    if (filtersMap.slots[slotIndex].key - 1 < 0) {
                        continue;
                    }

                    var filter = filters[slotIndex];

                    var match = true;
                    
                    for (var i = entityData.addedComponentsCount - 1; i >= 0; i--) {
                        if (filter.excludedTypeIdsLookup.IsSet(entityData.addedComponents[i])) {
                            match = false;
                            break;
                        }
                    }
                    
                    for (var i = entityData.removedComponentsCount - 1; i >= 0; i--) {
                        if (filter.includedTypeIdsLookup.IsSet(entityData.removedComponents[i])) {
                            match = false;
                            break;
                        }
                    }

                    if (!match) {
                        continue;
                    }

                    filter.AddArchetype(nextArchetype);
                    nextArchetype.AddFilter(filter);
                }
                
                entityData.currentArchetype.components.CopyTo(nextArchetype.components);
            }
            
            for (var i = entityData.addedComponentsCount - 1; i >= 0; i--) {
                nextArchetype.components.Add(entityData.addedComponents[i]);
            }
            
            for (var i = entityData.removedComponentsCount - 1; i >= 0; i--) {
                nextArchetype.components.Remove(entityData.removedComponents[i]);
            }
            
            // These checks must happen after setting components to new archetype
            // because we have to perform full check for filters

            for (var i = entityData.addedComponentsCount - 1; i >= 0; i--) {
                var filters = world.componentsFiltersWith.GetFilters(entityData.addedComponents[i]);
                if (filters == null) {
                    continue;
                }
                
                ScanFilters(nextArchetype, filters);
            }
            
            for (var i = entityData.removedComponentsCount - 1; i >= 0; i--) {
                var filters = world.componentsFiltersWithout.GetFilters(entityData.removedComponents[i]);
                if (filters == null) {
                    continue;
                }
                
                ScanFilters(nextArchetype, filters);
            }
            
            world.archetypes.Add(nextArchetype);
            world.archetypesCount++;
            
            return nextArchetype;
        }

        internal static void ScanFilters(Archetype archetype, Filter[] filters) {
            for (var i = filters.Length - 1; i >= 0; i--) {
                var filter = filters[i];
                
                if (filter.includedTypeIds.Length > archetype.components.length) {
                    continue;
                }
                    
                if (filter.AddArchetypeIfMatches(archetype)) {
                    archetype.AddFilter(filter);
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CompleteEntityDisposal(this World world, int entityId, ref EntityData entityData) {
            if (entityData.currentArchetype != null) {
                var index = entityData.indexInCurrentArchetype;
                entityData.currentArchetype.RemoveEntityAtIndex(index);
                
                var entityIndex = entityData.currentArchetype.entities[index].Id;
                world.entities[entityIndex].indexInCurrentArchetype = index;
                
                world.TryScheduleArchetypeForRemoval(entityData.currentArchetype);
            }

            entityData.currentArchetype = null;
            entityData.indexInCurrentArchetype = -1;
            
            entityData.addedComponentsCount = 0;
            entityData.removedComponentsCount = 0;
            
            entityData.nextArchetypeHash = default;
            
            world.freeEntityIDs.Push(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IncrementGeneration(this World world, int entityId) {
            unchecked {
                world.entitiesGens[entityId]++;
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
                ThreadSafetyCheckFailedException.Throw(currentThread, world.threadIdLock);
            }
        }
        
        [PublicAPI]
        [Obsolete("Will be removed in future versions")]
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
