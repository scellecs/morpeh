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
    using UnityEngine;

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class World : IDisposable {
        [CanBeNull]
        public static World Default => worlds.data[0];
        [NotNull]
        internal static FastList<World> worlds = new FastList<World> { null };

        [NonSerialized]
        public Filter Filter;
        [SerializeField]
        public bool UpdateByUnity;

        [NonSerialized]
        internal FastList<Filter> filters;

        //todo custom collection
        [ShowInInspector]
        [NonSerialized]
        internal SortedList<int, SystemsGroup> systemsGroups;
        
        //todo custom collection
        [ShowInInspector]
        [NonSerialized]
        internal FastList<SystemsGroup> pluginSystemsGroups;

        //todo custom collection
        [ShowInInspector]
        [NonSerialized]
        internal SortedList<int, SystemsGroup> newSystemsGroups;
        
        //todo custom collection
        [ShowInInspector]
        [NonSerialized]
        internal FastList<SystemsGroup> newPluginSystemsGroups;

        [SerializeField]
        internal Entity[] entities;

        [SerializeField]
        internal int[] entitiesGens;
        
        //real entities count
        [SerializeField]
        internal int entitiesCount;
        //count + unused slots
        [SerializeField]
        internal int entitiesLength;
        //all possible slots
        [SerializeField]
        internal int entitiesCapacity;

        [NonSerialized]
        internal BitMap dirtyEntities;

        [SerializeField]
        internal IntFastList freeEntityIDs;
        [SerializeField]
        internal IntFastList nextFreeEntityIDs;

        [SerializeField]
        internal UnsafeIntHashMap<int> stashes;
        [SerializeField]
        internal UnsafeIntHashMap<int> typedStashes;

        [SerializeField]
        internal FastList<Archetype> archetypes;
        [SerializeField]
        internal IntHashMap<IntFastList> archetypesByLength;
        [SerializeField]
        internal IntFastList newArchetypes;
        [NonSerialized]
        internal IntFastList archetypeCache;

        [SerializeField]
        internal int identifier;

        [SerializeField]
        internal string friendlyName;

        internal int threadIdLock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World Create() => new World().Initialize();

        public static World Create(string friendlyName) {
            var world = Create();
            world.SetFriendlyName(friendlyName);
            return world;
        }

        private World() => this.Ctor();

        //todo rework defines to conditionals
        public void Dispose() {
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
#if MORPEH_DEBUG
            try {
#endif
                this.Filter.Dispose();
#if MORPEH_DEBUG
            }
            catch (Exception e) {
                MLogger.LogError("Can not dispose root filter");
                MLogger.LogException(e);
            }
#endif
            this.Filter = null;

            this.filters.Clear();
            this.filters = null;

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
                    archetype.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose archetype id {archetype.id}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.archetypes.Clear();
            this.archetypes = null;

            foreach (var index in this.archetypesByLength) {
                this.archetypesByLength.GetValueByIndex(index).Clear();
            }

            this.archetypesByLength.Clear();
            this.archetypesByLength = null;

            this.newArchetypes.Clear();
            this.newArchetypes = null;

            worlds.Remove(this);
        }
    }
}
