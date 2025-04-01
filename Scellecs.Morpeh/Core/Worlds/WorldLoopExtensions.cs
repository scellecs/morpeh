namespace Scellecs.Morpeh {
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class WorldLoopExtensions {
        [PublicAPI]
        public static void GlobalUpdate(float deltaTime) {
            var count = World.worldsCount;
            for (int i = 0; i < count; i++) {
                var world = World.worlds[i];
                if (!world.IsNullOrDisposed() && world.UpdateByUnity) {
                    world.Update(deltaTime);
                }
            }
        }

        [PublicAPI]
        public static void GlobalFixedUpdate(float deltaTime) {
            var count = World.worldsCount;
            for (int i = 0; i < count; i++) {
                var world = World.worlds[i];
                if (!world.IsNullOrDisposed() && world.UpdateByUnity) {
                    world.FixedUpdate(deltaTime);
                }
            }
        }

        [PublicAPI]
        public static void GlobalLateUpdate(float deltaTime) {
            var count = World.worldsCount;
            for (int i = 0; i < count; i++) {
                var world = World.worlds[i];
                if (!world.IsNullOrDisposed() && world.UpdateByUnity) {
                    world.LateUpdate(deltaTime);
                    world.CleanupUpdate(deltaTime);
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
            m.filters = world.filterCount;
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
    }
}