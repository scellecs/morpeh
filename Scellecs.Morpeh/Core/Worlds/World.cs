#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
#if MORPEH_BURST
    using Unity.Jobs;
    using Unity.Collections;
#endif

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed partial class World : IDisposable {
        [CanBeNull]
        [PublicAPI]
        public static World Default {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => defaultWorld;
        }
        [CanBeNull]
        internal static World defaultWorld;
        [NotNull]
        [PublicAPI]
        internal static FastList<World> worlds = new FastList<World>();
        [NotNull]
        [PublicAPI]
        internal static IntStack freeWorldIDs = new IntStack();
        [NotNull]
        [PublicAPI]
        internal static byte[] worldsGens = new byte[4];

        internal static int worldsCount = 0;
        
        [CanBeNull]
        internal static FastList<IWorldPlugin> plugins;

        internal int filterCount;

        [PublicAPI]
        [NotNull]
        public FilterBuilder Filter => FilterBuilder.Create(this);
        
        [PublicAPI]
        public bool UpdateByUnity;
        
        [PublicAPI]
        public bool DoNotDisableSystemOnException;
        
        [PublicAPI]
        public bool IsDisposed;
#if MORPEH_BURST
        [PublicAPI]
        public JobHandle JobHandle;
#endif
        internal LongHashMap<LongHashMap<Filter>> filtersLookup;

        internal IntStack freeFilterIDs;

        //todo custom collection
        [ShowInInspector]
        internal SortedList<int, SystemsGroup> systemsGroups;
        
        //todo custom collection
        [ShowInInspector]
        internal FastList<SystemsGroup> pluginSystemsGroups;

        //todo custom collection
        [ShowInInspector]
        internal SortedList<int, SystemsGroup> newSystemsGroups;
        
        //todo custom collection
        [ShowInInspector]
        internal FastList<SystemsGroup> newPluginSystemsGroups;

        [ShowInInspector]
        internal EntityData[] entities;
        
        [ShowInInspector]
        internal ushort[] entitiesGens;
        
        //real entities count
        [ShowInInspector]
        internal int entitiesCount;
        //count + unused slots
        [ShowInInspector]
        internal int entitiesLength;
        //all possible slots
        [ShowInInspector]
        internal int entitiesCapacity;

        internal IntSparseSet dirtyEntities;
        internal IntSparseSet disposedEntities;

        [ShowInInspector]
        internal IntStack freeEntityIDs;

        [ShowInInspector]
        internal IStash[] stashes;

        [ShowInInspector]
        internal LongHashMap<Archetype> archetypes;
        [ShowInInspector]
        internal int archetypesCount;
        
        internal ArchetypePool archetypePool;
        
        [ShowInInspector]
        internal FastList<Archetype> emptyArchetypes;
        
        [ShowInInspector]
        internal int identifier;

        [ShowInInspector]
        internal int generation;
        
        [ShowInInspector]
        internal int threadIdLock;
        
#if MORPEH_DEBUG
        [ShowInInspector]
        internal int iteratorLevel;
#endif
        [ShowInInspector]
        public Metrics metrics;
        internal Metrics newMetrics;
        
        internal ComponentsToFiltersRelation componentsFiltersWith;
        internal ComponentsToFiltersRelation componentsFiltersWithout;

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World Create() {
            if (worldsCount == WorldConstants.MAX_WORLDS_COUNT) {
#if MORPEH_DEBUG
                MLogger.LogError($"Can not create a world, as the number of worlds has reached the limit of {WorldConstants.MAX_WORLDS_COUNT}");
#endif
                return null;
            }

            return new World().Initialize();
        }

        private World() {
            this.threadIdLock = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            this.systemsGroups = new SortedList<int, SystemsGroup>();
            this.newSystemsGroups = new SortedList<int, SystemsGroup>();

            this.pluginSystemsGroups = new FastList<SystemsGroup>();
            this.newPluginSystemsGroups = new FastList<SystemsGroup>();

            this.filtersLookup = new LongHashMap<LongHashMap<Filter>>();
            this.freeFilterIDs = new IntStack();
        }

        [PublicAPI]
        public void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            
            if (plugins != null) {
                foreach (var plugin in plugins) {
#if MORPEH_DEBUG
                    try {
#endif
                        plugin.Deinitialize(this);
#if MORPEH_DEBUG
                    }
                    catch (Exception e) {
                        MLogger.LogError($"Can not deinitialize world plugin {plugin.GetType()}");
                        MLogger.LogException(e);
                    }
#endif
                }
            }
            
            foreach (var systemsGroup in this.systemsGroups.Values) {
#if MORPEH_DEBUG
                try {
#endif
                    systemsGroup.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose system group {systemsGroup.GetType()}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.newSystemsGroups.Clear();
            this.newSystemsGroups = null;
            
            this.systemsGroups.Clear();
            this.systemsGroups = null;
            
            foreach (var systemsGroup in this.pluginSystemsGroups) {
#if MORPEH_DEBUG
                try {
#endif
                    systemsGroup.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose plugin system group {systemsGroup.GetType()}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.newPluginSystemsGroups.Clear();
            this.newPluginSystemsGroups = null;
            
            this.pluginSystemsGroups.Clear();
            this.pluginSystemsGroups = null;

            this.entities         = null;
            this.entitiesCount    = -1;
            this.entitiesLength   = -1;
            this.entitiesCapacity = -1;

            this.entitiesGens     = null;

            this.freeEntityIDs.Clear();
            this.freeEntityIDs = null;
            
            this.filterCount = 0;            
            this.filtersLookup = null;
            this.freeFilterIDs.Clear();

            foreach (var stash in this.stashes) {
#if MORPEH_DEBUG
                try {
#endif
                    stash?.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose stash with type id {stash.Type}");
                    MLogger.LogException(e);
                }
#endif
            }
            this.stashes = null;

            foreach (var archetype in this.archetypes) {
#if MORPEH_DEBUG
                try {
#endif
                    this.archetypes.GetValueByIndex(archetype).Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose archetype id {archetype}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.archetypes.Clear();
            this.archetypes = null;
            
            this.emptyArchetypes.Clear();
            this.emptyArchetypes = null;
            
            this.archetypePool.Dispose();
            this.archetypePool = default;
            
            this.IsDisposed = true;

            this.ApplyRemoveWorld();
        }

        public struct Metrics {
            public int entities;
            public int archetypes;
            public int filters;
            public int systems;
            public int commits;
            public int migrations;
            public int stashResizes;
        }
    }
}