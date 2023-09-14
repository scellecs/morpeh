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
        public static World Default => worlds.data[0];
        [NotNull]
        [PublicAPI]
        internal static FastList<World> worlds = new FastList<World>().WithElement(null);
        
        [CanBeNull]
        internal static FastList<IWorldPlugin> plugins;

        [PublicAPI]
        [NotNull]
        public FilterBuilder Filter;
        [PublicAPI]
        public bool UpdateByUnity;
        [PublicAPI]
        public bool DoNotDisableSystemOnException;
        [PublicAPI]
        public bool IsDisposed;
#if MORPEH_BURST
        [PublicAPI]
        public JobHandle JobHandle;
        //todo will be replaced with smart enumerator
        internal FastList<NativeArray<int>> tempArrays;
#endif
        internal FastList<Filter> filters;
        internal LongHashMap<FilterNode> filtersTree;
        internal LongHashMap<LongHashMap<Filter>> filtersLookup;

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
        internal Entity[] entities;

        [ShowInInspector]
        internal int[] entitiesGens;
        
        //real entities count
        [ShowInInspector]
        internal int entitiesCount;
        //count + unused slots
        [ShowInInspector]
        internal int entitiesLength;
        //all possible slots
        [ShowInInspector]
        internal int entitiesCapacity;

        internal BitMap dirtyEntities;

        [ShowInInspector]
        internal IntStack freeEntityIDs;
        [ShowInInspector]
        internal IntStack nextFreeEntityIDs;

        [ShowInInspector]
        internal LongHashMap<int> stashes;
        [ShowInInspector]
        internal LongHashMap<int> typedStashes;

        [ShowInInspector]
        internal LongHashMap<Archetype> archetypes;
        [ShowInInspector]
        internal int archetypesCount;
        
        [ShowInInspector]
        internal FastList<Archetype> removedArchetypes;
        [ShowInInspector]
        internal FastList<Archetype> emptyArchetypes;
        
        internal FastList<long> archetypeCache;

        [ShowInInspector]
        internal int identifier;

        [ShowInInspector]
        internal string friendlyName;

        [ShowInInspector]
        internal int threadIdLock;
        
#if MORPEH_DEBUG
        [ShowInInspector]
        internal int iteratorLevel;
#endif
        [ShowInInspector]
        public Metrics metrics;
        internal Metrics newMetrics;

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World Create() => new World().Initialize();

        [PublicAPI]
        public static World Create(string friendlyName) {
            var world = Create();
            world.SetFriendlyName(friendlyName);
            return world;
        }

        private World() => this.Ctor();

        //todo rework defines to conditionals
        [PublicAPI]
        public void Dispose() {
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

            foreach (var entity in this.entities) {
#if MORPEH_DEBUG
                try {
#endif
                    entity?.DisposeFast();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose entity with ID {entity?.entityId}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.entities         = null;
            this.entitiesCount    = -1;
            this.entitiesLength   = -1;
            this.entitiesCapacity = -1;

            this.freeEntityIDs.Clear();
            this.freeEntityIDs = null;
            this.nextFreeEntityIDs.Clear();
            this.nextFreeEntityIDs = null;
            
            this.Filter = null;

            this.filters.Clear();
            this.filters = null;
            
            this.filtersTree.Clear();
            this.filtersTree = null;
            
            this.filtersLookup.Clear();
            this.filtersLookup = null;

            var tempStashes = new FastList<Stash>();

            foreach (var stashId in this.stashes) {
                var stash = Stash.stashes.data[this.stashes.GetValueByIndex(stashId)];
                tempStashes.Add(stash);
            }

            foreach (var stash in tempStashes) {
#if MORPEH_DEBUG
                try {
#endif
                    stash.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose stash with id {stash.commonStashId}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.stashes.Clear();
            this.stashes = null;
            this.typedStashes.Clear();
            this.typedStashes = null;

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
            
            this.removedArchetypes.Clear();
            this.removedArchetypes = null;

            worlds.Remove(this);
            
            this.IsDisposed = true;
        }

        public struct Metrics {
            public int entities;
            public int archetypes;
            public int filters;
            public int systems;
            public int commits;
            public int migrations;
        }
    }
}
