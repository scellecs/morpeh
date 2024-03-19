#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
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
            if (systemsGroup.disposables.length == 0 && systemsGroup.newInitializers.length == 0) {
                return;
            }
            
            MLogger.BeginSample("SystemGroup.Initialize()");
            systemsGroup.DropDelayedAction();
            
            if (systemsGroup.disposables.length > 0) {
                systemsGroup.world.Commit();

                foreach (var disposable in systemsGroup.disposables) {
                    disposable.TryCatchDispose();
                    disposable.ForwardDispose();

                    systemsGroup.world.Commit();
                }

                systemsGroup.disposables.Clear();
            }

            systemsGroup.world.Commit();
            if (systemsGroup.newInitializers.length > 0) {
                foreach (var initializer in systemsGroup.newInitializers) {
                    initializer.TryCatchAwake();
                    initializer.ForwardAwake();

                    systemsGroup.world.Commit();
                    systemsGroup.initializers.Add(initializer);
                }

                systemsGroup.newInitializers.Clear();
            }
            systemsGroup.InvokeDelayedAction();
            systemsGroup.world.JobsComplete();
            MLogger.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(this SystemsGroup systemsGroup, float deltaTime) {
            systemsGroup.Initialize();

            if (systemsGroup.systems.length == 0) {
                return;
            }
            
            MLogger.BeginSample("SystemGroup.Update()");
            systemsGroup.DropDelayedAction();
            systemsGroup.world.Commit();
            
            for (int i = 0, length = systemsGroup.systems.length; i < length; i++) {
                var system = systemsGroup.systems.data[i];

                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.Commit();
            }

            systemsGroup.InvokeDelayedAction();
            systemsGroup.world.JobsComplete();
            MLogger.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FixedUpdate(this SystemsGroup systemsGroup, float deltaTime) {
            if (systemsGroup.fixedSystems.length == 0) {
                return;
            }
            MLogger.BeginSample("SystemGroup.FixedUpdate()");
            systemsGroup.DropDelayedAction();
            systemsGroup.world.Commit();

            for (int i = 0, length = systemsGroup.fixedSystems.length; i < length; i++) {
                var system = systemsGroup.fixedSystems.data[i];

                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.Commit();
            }

            systemsGroup.InvokeDelayedAction();
            systemsGroup.world.JobsComplete();
            MLogger.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LateUpdate(this SystemsGroup systemsGroup, float deltaTime) {
            if (systemsGroup.lateSystems.length == 0) {
                return;
            }
            
            MLogger.BeginSample("SystemGroup.LateUpdate()");
            systemsGroup.DropDelayedAction();
            systemsGroup.world.Commit();

            for (int i = 0, length = systemsGroup.lateSystems.length; i < length; i++) {
                var system = systemsGroup.lateSystems.data[i];
                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.Commit();
            }

            systemsGroup.InvokeDelayedAction();
            systemsGroup.world.JobsComplete();
            MLogger.EndSample();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CleanupUpdate(this SystemsGroup systemsGroup, float deltaTime) {
            if (systemsGroup.cleanupSystems.length == 0) {
                return;
            }
            
            MLogger.BeginSample("SystemGroup.CleanupUpdate()");
            systemsGroup.DropDelayedAction();
            systemsGroup.world.Commit();

            for (int i = 0, length = systemsGroup.cleanupSystems.length; i < length; i++) {
                var system = systemsGroup.cleanupSystems.data[i];
                system.TryCatchUpdate(systemsGroup, deltaTime);
                system.ForwardUpdate(deltaTime);

                systemsGroup.world.Commit();
            }

            systemsGroup.InvokeDelayedAction();
            systemsGroup.world.JobsComplete();
            MLogger.EndSample();
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
            if (systemsGroup.world.DoNotDisableSystemOnException) {
                MLogger.LogError($"Can not update {system.GetType()}.");
                MLogger.LogException(exception);
            }
            else {
                MLogger.LogError($"Can not update {system.GetType()}. System will be disabled.");
                MLogger.LogException(exception);
                systemsGroup.delayedAction += () => systemsGroup.DisableSystem(system);
            }
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryCatchUpdate(this ISystem system, SystemsGroup systemsGroup, float deltaTime) {
            try {
#if MORPEH_PROFILING
                using (new ProfilerSampler(system)) {
#endif
                    system.OnUpdate(deltaTime);
#if MORPEH_PROFILING
                }
#endif
            }
            catch (Exception exception) {
                systemsGroup.SystemThrowException(system, exception);
            }
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryCatchAwake(this IInitializer initializer) {
            try {
#if MORPEH_PROFILING
                using (new ProfilerSampler(initializer)) {
#endif
                    initializer.OnAwake();
#if MORPEH_PROFILING
                }
#endif
            }
            catch (Exception exception) {
                MLogger.LogError($"Can not initialize {initializer.GetType()}");
                MLogger.LogException(exception);
            }
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryCatchDispose(this IDisposable disposable) {
            try {
#if MORPEH_PROFILING
                using (new ProfilerSampler(disposable)) {
#endif
                    disposable.Dispose();
#if MORPEH_PROFILING
                }
#endif
            }
            catch (Exception exception) {
                MLogger.LogError($"Can not dispose {disposable.GetType()}");
                MLogger.LogException(exception);
            }
        }

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ForwardDispose(this IDisposable disposable) {
#if MORPEH_PROFILING
            using (new ProfilerSampler(disposable)) {
#endif
                disposable.Dispose();
#if MORPEH_PROFILING
            }
#endif
        }

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ForwardAwake(this IInitializer initializer) {
#if MORPEH_PROFILING
            using (new ProfilerSampler(initializer)) {
#endif
                initializer.OnAwake();
#if MORPEH_PROFILING
            }
#endif
        }

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ForwardUpdate(this ISystem system, float deltaTime) {
#if MORPEH_PROFILING
            using (new ProfilerSampler(system)) {
#endif
                system.OnUpdate(deltaTime);
#if MORPEH_PROFILING
            }
#endif
        }

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
            else if (system is ICleanupSystem) {
                collection         = systemsGroup.cleanupSystems;
                disabledCollection = systemsGroup.disabledCleanupSystems;
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
            else if (system is ICleanupSystem) {
                collection         = systemsGroup.cleanupSystems;
                disabledCollection = systemsGroup.disabledCleanupSystems;
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
            else if (system is ICleanupSystem) {
                collection         = systemsGroup.cleanupSystems;
                disabledCollection = systemsGroup.disabledCleanupSystems;
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
            else if (system is ICleanupSystem) {
                collection         = systemsGroup.cleanupSystems;
                disabledCollection = systemsGroup.disabledCleanupSystems;
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

#if MORPEH_PROFILING
        private struct ProfilerSampler : IDisposable {

            internal static IntHashMap<string> debugInstanceToString = new IntHashMap<string>();

            public ProfilerSampler(object obj) {
                if (!debugInstanceToString.TryGetValue(obj.GetHashCode(), out var str)) {
                    str = obj.GetType().FullName;
                    debugInstanceToString.Add(obj.GetHashCode(), str, out _);
                }
                MLogger.BeginSample(str);
            }
            public void Dispose() {
                MLogger.EndSample();
            }
        }
#endif
    }
}
