namespace Morpeh.Globals {
    namespace ECS {
        using System;
        using System.Collections.Generic;
        using UnityEngine;

        [Serializable]
        internal struct GlobalEventMarker : IComponent {
        }

        internal abstract class GlobalEventComponentUpdater {
            internal static List<GlobalEventComponentUpdater> Updaters = new List<GlobalEventComponentUpdater>();

            protected Filter filter;
            protected Filter filterNextFrame;


            internal abstract void Update(World world);
        }

        internal sealed class GlobalEventComponentUpdater<T> : GlobalEventComponentUpdater {
            internal override void Update(World world) {
                var common = world.Filter.With<GlobalEventMarker>().With<GlobalEventComponent<T>>();
                foreach (var entity in common.With<GlobalEventPublished>().Without<GlobalEventNextFrame>()) {
                    ref var evnt = ref entity.GetComponent<GlobalEventComponent<T>>(out _);
                    evnt.Action?.Invoke(evnt.Data);
                    evnt.Data.Clear();
                    entity.RemoveComponent<GlobalEventPublished>();
                }
                foreach (var entity in common.With<GlobalEventPublished>().With<GlobalEventNextFrame>()) {
                    ref var evnt = ref entity.GetComponent<GlobalEventComponent<T>>(out _);
                    evnt.Action?.Invoke(evnt.Data);
                }
                foreach (var entity in common.With<GlobalEventNextFrame>()) {
                    entity.SetComponent(new GlobalEventPublished ());
                    entity.RemoveComponent<GlobalEventNextFrame>();
                }
            }
        }


        [Serializable]
        internal struct GlobalEventComponent<TData> : IComponent {
            internal static bool Initialized;

            public Action<IEnumerable<TData>> Action;
            public Stack<TData>               Data;
        }

        [Serializable]
        internal struct GlobalEventPublished : IComponent {
        }

        [Serializable]
        internal struct GlobalEventNextFrame : IComponent {
        }

        internal sealed class ProcessEventsSystem : ILateSystem {
            public World World { get; set; }

            public void OnAwake() {
            }

            public void OnUpdate(float deltaTime) {
                foreach (var updater in GlobalEventComponentUpdater.Updaters) {
                    updater.Update(this.World);
                }
            }

            public void Dispose() {
            }
        }
    }
}

namespace Morpeh {
    partial class World {
        partial void InitializeGlobals() {
            var sg = this.CreateSystemsGroup();
            sg.AddSystem(new Morpeh.Globals.ECS.ProcessEventsSystem());
            this.AddSystemsGroup(9000, sg);
        }
    }
}