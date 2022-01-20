#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Morpeh {
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class SystemsGroupExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize(this SystemsGroup systemsGroup) {
            if (systemsGroup.disposables.length > 0) {
                foreach (var disposable in systemsGroup.disposables) {
                    disposable.TryCatchDispose();
                    disposable.ForwardDispose();
                }

                systemsGroup.disposables.Clear();
            }

            systemsGroup.world.UpdateFilters();
            if (systemsGroup.newInitializers.length > 0) {
                foreach (var initializer in systemsGroup.newInitializers) {
                    initializer.TryCatchAwake();
                    initializer.ForwardAwake();

                    systemsGroup.world.UpdateFilters();
                    systemsGroup.initializers.Add(initializer);
                }

                systemsGroup.newInitializers.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(this SystemsGroup systemsGroup, float deltaTime) {
            systemsGroup.DropDelayedAction();

            systemsGroup.Initialize();
            for (int i = 0, length = systemsGroup.systems.length; i < length; i++) {
                var system = systemsGroup.systems.data[i];

                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.UpdateFilters();
            }

            systemsGroup.InvokeDelayedAction();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FixedUpdate(this SystemsGroup systemsGroup, float deltaTime) {
            systemsGroup.DropDelayedAction();
            for (int i = 0, length = systemsGroup.fixedSystems.length; i < length; i++) {
                var system = systemsGroup.fixedSystems.data[i];

                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.UpdateFilters();
            }

            systemsGroup.InvokeDelayedAction();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LateUpdate(this SystemsGroup systemsGroup, float deltaTime) {
            systemsGroup.DropDelayedAction();
            systemsGroup.world.UpdateFilters();

            for (int i = 0, length = systemsGroup.lateSystems.length; i < length; i++) {
                var system = systemsGroup.lateSystems.data[i];
                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.UpdateFilters();
            }

            systemsGroup.InvokeDelayedAction();
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DropDelayedAction(this SystemsGroup systemsGroup) {
            systemsGroup.delayedAction = null;
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InvokeDelayedAction(this SystemsGroup systemsGroup) {
            systemsGroup.delayedAction?.Invoke();
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SystemThrowException(this SystemsGroup systemsGroup, ISystem system, Exception exception) {
            MDebug.LogError($"Can not update {system.GetType()}. System will be disabled.");
            MDebug.LogException(exception);
            systemsGroup.delayedAction += () => systemsGroup.DisableSystem(system);
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryCatchUpdate(this ISystem system, SystemsGroup systemsGroup, float deltaTime) {
            try {
                system.OnUpdate(deltaTime);
            }
            catch (Exception exception) {
                systemsGroup.SystemThrowException(system, exception);
            }
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryCatchAwake(this IInitializer initializer) {
            try {
                initializer.OnAwake();
            }
            catch (Exception exception) {
                MDebug.LogError($"Can not initialize {initializer.GetType()}");
                MDebug.LogException(exception);
            }
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryCatchDispose(this IDisposable disposable) {
            try {
                disposable.Dispose();
            }
            catch (Exception exception) {
                MDebug.LogError($"Can not dispose {disposable.GetType()}");
                MDebug.LogException(exception);
            }
        }

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ForwardDispose(this IDisposable disposable) => disposable.Dispose();

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ForwardAwake(this IInitializer initializer) => initializer.OnAwake();

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ForwardUpdate(this ISystem system, float deltaTime) => system.OnUpdate(deltaTime);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInitializer<T>(this SystemsGroup systemsGroup, T initializer) where T : class, IInitializer {
            initializer.World = systemsGroup.world;

            systemsGroup.newInitializers.Add(initializer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveInitializer<T>(this SystemsGroup systemsGroup, T initializer) where T : class, IInitializer {
            var index = systemsGroup.newInitializers.IndexOf(initializer);
            if (index >= 0) {
                systemsGroup.newInitializers.RemoveAt(index);
            }

            index = systemsGroup.initializers.IndexOf(initializer);
            if (index >= 0) {
                systemsGroup.initializers.RemoveAt(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddSystem<T>(this SystemsGroup systemsGroup, T system, bool enabled = true) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            if (enabled && collection.IndexOf(system) < 0) {
                collection.Add(system);
                systemsGroup.AddInitializer(system);
                return true;
            }

            if (!enabled && disabledCollection.IndexOf(system) < 0) {
                disabledCollection.Add(system);
                systemsGroup.AddInitializer(system);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EnableSystem<T>(this SystemsGroup systemsGroup, T system) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            var index = disabledCollection.IndexOf(system);
            if (index >= 0) {
                collection.Add(system);
                disabledCollection.RemoveAt(index);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DisableSystem<T>(this SystemsGroup systemsGroup, T system) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            var index = collection.IndexOf(system);
            if (index >= 0) {
                disabledCollection.Add(system);
                collection.RemoveAt(index);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveSystem<T>(this SystemsGroup systemsGroup, T system) where T : class, ISystem {
            var collection         = systemsGroup.systems;
            var disabledCollection = systemsGroup.disabledSystems;
            if (system is IFixedSystem) {
                collection         = systemsGroup.fixedSystems;
                disabledCollection = systemsGroup.disabledFixedSystems;
            }
            else if (system is ILateSystem) {
                collection         = systemsGroup.lateSystems;
                disabledCollection = systemsGroup.disabledLateSystems;
            }

            var index = collection.IndexOf(system);
            if (index >= 0) {
                collection.RemoveAt(index);
                systemsGroup.disposables.Add(system);
                systemsGroup.RemoveInitializer(system);
                return true;
            }

            index = disabledCollection.IndexOf(system);
            if (index >= 0) {
                disabledCollection.RemoveAt(index);
                systemsGroup.disposables.Add(system);
                systemsGroup.RemoveInitializer(system);
                return true;
            }

            return false;
        }
    }
}
