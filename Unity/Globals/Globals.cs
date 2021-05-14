namespace Morpeh.Globals {
    namespace ECS {
        using System;
        using System.Collections.Generic;
        
        [Serializable]
        public struct GlobalEventMarker : IComponent {
        }

        internal abstract class GlobalEventComponentUpdater : IDisposable {
            internal static Dictionary<int, List<GlobalEventComponentUpdater>> updaters = new Dictionary<int, List<GlobalEventComponentUpdater>>();

            protected Filter filterPublishedWithoutNextFrame;
            protected Filter filterNextFrame;

            internal abstract void Awake(World world);

            internal abstract void Update();

            public abstract void Dispose();
        }

        internal sealed class GlobalEventComponentUpdater<T> : GlobalEventComponentUpdater {
            public static Dictionary<int, bool> initialized = new Dictionary<int, bool>();

            public int worldId;
            
            internal override void Awake(World world) {
                this.worldId = world.identifier;

                if (initialized.ContainsKey(this.worldId)) {
                    initialized[this.worldId] = true;
                }
                else {
                    initialized.Add(this.worldId, true);
                }
                
                var common = world.Filter.With<GlobalEventMarker>().With<GlobalEventComponent<T>>();
                this.filterPublishedWithoutNextFrame = common.With<GlobalEventPublished>();
                this.filterNextFrame = common.With<GlobalEventNextFrame>();
            }

            internal override void Update() {
                foreach (var entity in this.filterPublishedWithoutNextFrame) {
                    ref var evnt = ref entity.GetComponent<GlobalEventComponent<T>>(out _);
                    evnt.Action?.Invoke(evnt.Data);
                    evnt.Data.Clear();
                    while (evnt.NewData.Count > 0) {
                        evnt.Data.Push(evnt.NewData.Dequeue());
                    }
                    entity.RemoveComponent<GlobalEventPublished>();
                }
                foreach (var entity in this.filterNextFrame) {
                    entity.SetComponent(new GlobalEventPublished ());
                    entity.RemoveComponent<GlobalEventNextFrame>();
                }
            }

            public override void Dispose() {
                initialized[this.worldId] = false;
            }
        }


        [Serializable]
        public struct GlobalEventComponent<TData> : IComponent {
            public Action<IEnumerable<TData>> Action;
            public Stack<TData>               Data;
            public Queue<TData>               NewData;
        }
        [Serializable]
        public struct GlobalEventLastToString : IComponent {
            public Func<string> LastToString;
        }

        [Serializable]
        public struct GlobalEventPublished : IComponent {
        }

        [Serializable]
        public struct GlobalEventNextFrame : IComponent {
        }

        internal sealed class ProcessEventsSystem : ILateSystem {
            public World World { get; set; }
            public int worldId;

            public void OnAwake() {
                this.worldId = this.World.identifier;
            }

            public void OnUpdate(float deltaTime) {
                if (GlobalEventComponentUpdater.updaters.TryGetValue(this.worldId, out var updaters)) {
                    foreach (var updater in updaters) {
                        updater.Update();
                    }
                }
            }

            public void Dispose() {
                if (GlobalEventComponentUpdater.updaters.TryGetValue(this.worldId, out var updaters)) {
                    foreach (var updater in updaters) {
                        updater.Dispose();
                    }
                    updaters.Clear();
                }
            }
        }
    }
}

namespace Morpeh {
    partial class WorldExtensions {
        static partial void InitializeGlobals(this World world) {
            var sg = world.CreateSystemsGroup();
            sg.AddSystem(new Morpeh.Globals.ECS.ProcessEventsSystem());
            world.AddSystemsGroup(99999, sg);
        }
    }
}