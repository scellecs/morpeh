#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SystemsGroup : IDisposable {
        [ShowInInspector]
        internal FastList<ISystem> systems;
        [ShowInInspector]
        internal FastList<ISystem> fixedSystems;
        [ShowInInspector]
        internal FastList<ISystem> lateSystems;
        [ShowInInspector]
        internal FastList<ISystem> cleanupSystems;

        [ShowInInspector]
        internal FastList<ISystem> disabledSystems;
        [ShowInInspector]
        internal FastList<ISystem> disabledFixedSystems;
        [ShowInInspector]
        internal FastList<ISystem> disabledLateSystems;
        [ShowInInspector]
        internal FastList<ISystem> disabledCleanupSystems;

        [ShowInInspector]
        internal FastList<IInitializer> newInitializers;
        [ShowInInspector]
        internal FastList<IInitializer> initializers;
        [ShowInInspector]
        internal FastList<IDisposable> disposables;

        internal World  world;
        internal Action delayedAction;

        private SystemsGroup() {
        }

        internal SystemsGroup(World world) {
            this.world         = world;
            this.delayedAction = null;

            this.systems         = new FastList<ISystem>();
            this.fixedSystems    = new FastList<ISystem>();
            this.lateSystems     = new FastList<ISystem>();
            this.cleanupSystems  = new FastList<ISystem>();

            this.disabledSystems         = new FastList<ISystem>();
            this.disabledFixedSystems    = new FastList<ISystem>();
            this.disabledLateSystems     = new FastList<ISystem>();
            this.disabledCleanupSystems  = new FastList<ISystem>();

            this.newInitializers = new FastList<IInitializer>();
            this.initializers    = new FastList<IInitializer>();
            this.disposables     = new FastList<IDisposable>();
        }

        public void Dispose() {
            if (this.disposables == null) {
                return;
            }

            void DisposeSystems(FastList<ISystem> systemsToDispose) {
                foreach (var system in systemsToDispose) {
#if MORPEH_DEBUG
                    try {
#endif
                        this.world.Commit();
                        system.Dispose();
#if MORPEH_DEBUG
                    }
                    catch (Exception e) {
                        MLogger.LogError($"Can not dispose system {system.GetType()}");
                        MLogger.LogException(e);
                    }
#endif
                    this.RemoveInitializer(system);
                }

                systemsToDispose.Clear();
            }

            DisposeSystems(this.systems);
            this.systems = null;

            DisposeSystems(this.fixedSystems);
            this.fixedSystems = null;

            DisposeSystems(this.lateSystems);
            this.lateSystems = null;

            DisposeSystems(this.cleanupSystems);
            this.cleanupSystems = null;

            DisposeSystems(this.disabledSystems);
            this.disabledSystems = null;

            DisposeSystems(this.disabledFixedSystems);
            this.disabledFixedSystems = null;

            DisposeSystems(this.disabledLateSystems);
            this.disabledLateSystems = null;

            DisposeSystems(this.disabledCleanupSystems);
            this.disabledCleanupSystems = null;

            foreach (var initializer in this.newInitializers) {
#if MORPEH_DEBUG
                try {
#endif
                    this.world.Commit();
                    initializer.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose new initializer {initializer.GetType()}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.newInitializers.Clear();
            this.newInitializers = null;

            foreach (var initializer in this.initializers) {
#if MORPEH_DEBUG
                try {
#endif
                    this.world.Commit();
                    initializer.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose initializer {initializer.GetType()}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.initializers.Clear();
            this.initializers = null;

            foreach (var disposable in this.disposables) {
#if MORPEH_DEBUG
                try {
#endif
                    this.world.Commit();
                    disposable.Dispose();
#if MORPEH_DEBUG
                }
                catch (Exception e) {
                    MLogger.LogError($"Can not dispose system group disposable {disposable.GetType()}");
                    MLogger.LogException(e);
                }
#endif
            }

            this.disposables.Clear();
            this.disposables = null;
        }
    }
}
