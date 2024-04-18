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

            world.archetypes         = new LongHashMap<Archetype>();
            world.archetypesCount    = 0;
            
            world.archetypePool = new ArchetypePool(32);
            world.emptyArchetypes = new FastList<Archetype>();

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
            foreach (var entityId in world.dirtyEntities) {
                ref var entityData = ref world.entities[entityId];
                
                if (entityData.nextArchetypeHash == default) {
                    world.CompleteEntityDisposal(entityId, ref entityData);
                    world.IncrementGeneration(entityId);
                    --world.entitiesCount;
                } else if (entityData.addedComponentsCount + entityData.removedComponentsCount > 0) {
                    world.ApplyTransientChanges(entityId, ref entityData);
                }
            }

            world.dirtyEntities.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ClearEmptyArchetypes(this World world) {
            foreach (var archetype in world.emptyArchetypes) {
                if (!archetype.IsEmpty()) {
                    MLogger.LogTrace($"[WorldExtensions] Archetype {archetype.hash} is not empty after complete migration of entities");
                    continue;
                }
                
                MLogger.LogTrace($"[WorldExtensions] Remove archetype {archetype.hash}");
                
                foreach (var idx in archetype.filters) {
                    var filter = archetype.filters.GetValueByIndex(idx);
                    filter.RemoveArchetype(archetype);
                }
                
                archetype.ClearFilters();
                archetype.components.Clear();
                
                world.archetypes.Remove(archetype.hash.GetValue(), out _);
                world.archetypesCount--;
                world.archetypePool.Return(archetype);
            }
            
            world.emptyArchetypes.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeAddComponent(this World world, int entityId, ref TypeInfo typeInfo) {
            ref var entityData = ref world.entities[entityId];
            entityData.nextArchetypeHash = entityData.nextArchetypeHash.Combine(typeInfo.hash);
            
            for (int i = 0, length = entityData.removedComponentsCount; i < length; i++) {
                if (entityData.removedComponents[i] != typeInfo.id) {
                    continue;
                }
                
                entityData.removedComponents[i] = entityData.removedComponents[--entityData.removedComponentsCount];
                return;
            }
            
            if (entityData.addedComponentsCount == entityData.addedComponents.Length) {
                ArrayHelpers.Grow(ref entityData.addedComponents, entityData.addedComponentsCount << 1);
            }

            entityData.addedComponents[entityData.addedComponentsCount++] = typeInfo.id;
            world.dirtyEntities.Add(entityId);
            
            MLogger.LogTrace($"[AddComponent] To: {entityData.nextArchetypeHash}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TransientChangeRemoveComponent(this World world, int entityId, ref TypeInfo typeInfo) {
            ref var entityData = ref world.entities[entityId];
            entityData.nextArchetypeHash = entityData.nextArchetypeHash.Combine(typeInfo.hash);
            
            for (int i = 0, length = entityData.addedComponentsCount; i < length; i++) {
                if (entityData.addedComponents[i] != typeInfo.id) {
                    continue;
                }
                
                entityData.addedComponents[i] = entityData.addedComponents[--entityData.addedComponentsCount];
                return;
            }
            
            if (entityData.removedComponentsCount == entityData.removedComponents.Length) {
                ArrayHelpers.Grow(ref entityData.removedComponents, entityData.removedComponentsCount << 1);
            }
            
            entityData.removedComponents[entityData.removedComponentsCount++] = typeInfo.id;
            world.dirtyEntities.Add(entityId);
            
            MLogger.LogTrace($"[RemoveComponent] To: {entityData.nextArchetypeHash}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransientChanges(this World world, int entityId, ref EntityData entityData) {
            // Add to new archetype
            if (!world.archetypes.TryGetValue(entityData.nextArchetypeHash.GetValue(), out var nextArchetype)) {
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
            world.emptyArchetypes.Add(archetype);
        }
        
        internal static Archetype CreateMigratedArchetype(this World world, ref EntityData entityData) {
            var nextArchetype = world.archetypePool.Rent(entityData.nextArchetypeHash);
            
            if (entityData.currentArchetype != null) {
                // These checks can happen before setting components to new archetype
                // because we only check for added/removed components
                
                var filters = entityData.currentArchetype.filters;
            
                foreach (var idx in filters) {
                    var filter = filters.GetValueByIndex(idx);

                    var match = true;
                    
                    for (int i = 0, length = entityData.addedComponentsCount; i < length; ++i) {
                        if (filter.excludedTypeIdsLookup.IsSet(entityData.addedComponents[i])) {
                            match = false;
                            break;
                        }
                    }
                    
                    for (int i = 0, length = entityData.removedComponentsCount; i < length; ++i) {
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
            
            for (int i = 0, length = entityData.addedComponentsCount; i < length; ++i) {
                nextArchetype.components.Add(entityData.addedComponents[i]);
            }
            
            for (int i = 0, length = entityData.removedComponentsCount; i < length; ++i) {
                nextArchetype.components.Remove(entityData.removedComponents[i]);
            }
            
            // These checks must happen after setting components to new archetype
            // because we have to perform full check for filters

            for (int i = 0, length = entityData.addedComponentsCount; i < length; ++i) {
                var filters = world.componentsFiltersWith.GetFilters(entityData.addedComponents[i]);
                if (filters == null) {
                    continue;
                }
                
                ScanFilters(nextArchetype, filters);
            }
            
            for (int i = 0, length = entityData.removedComponentsCount; i < length; ++i) {
                var filters = world.componentsFiltersWithout.GetFilters(entityData.removedComponents[i]);
                if (filters == null) {
                    continue;
                }
                
                ScanFilters(nextArchetype, filters);
            }
            
            world.archetypes.Add(nextArchetype.hash.GetValue(), nextArchetype, out _);
            world.archetypesCount++;
            
            return nextArchetype;
        }

        internal static void ScanFilters(Archetype archetype, Filter[] filters) {
            for (int i = 0, length = filters.Length; i < length; i++) {
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
