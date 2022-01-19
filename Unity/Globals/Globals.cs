namespace Morpeh.Globals {
    namespace ECS {
        using System;
        using System.Collections.Generic;
        using Collections;
        using UnityEngine;

        [Serializable]
        public struct GlobalEventMarker : IComponent {
        }

        //rework for multithread
        internal abstract class GlobalEventComponentUpdater : IDisposable {
            internal static Dictionary<int, List<GlobalEventComponentUpdater>> updaters = new Dictionary<int, List<GlobalEventComponentUpdater>>();

            protected Filter filterPublished;
            protected Filter filterNextFrame;

            internal abstract void Awake(World world);

            internal abstract void Update();

            public abstract void Dispose();
        }

        internal sealed class GlobalEventComponentUpdater<T> : GlobalEventComponentUpdater {
            public static BitMap initialized = new BitMap();

            private int worldId;
            
            private ComponentsCache<GlobalEventComponent<T>> eventsCache;
            private ComponentsCache<GlobalEventPublished>    publishedCache;
            private ComponentsCache<GlobalEventNextFrame>    nextFrameCache;
            
            internal override void Awake(World world) {
                this.worldId = world.identifier;
                
                initialized.Set(this.worldId);
                
                var common = world.Filter.With<GlobalEventMarker>().With<GlobalEventComponent<T>>();
                this.filterPublished = common.With<GlobalEventPublished>().Without<GlobalEventNextFrame>();
                this.filterNextFrame = common.With<GlobalEventNextFrame>();

                this.eventsCache    = world.GetCache<GlobalEventComponent<T>>();
                this.publishedCache = world.GetCache<GlobalEventPublished>();
                this.nextFrameCache = world.GetCache<GlobalEventNextFrame>();
            }

            internal override void Update() {
                foreach (var entity in this.filterPublished) {
                    ref var evnt = ref this.eventsCache.GetComponent(entity);
                    evnt.Action?.Invoke(evnt.Data);
                    evnt.Data.Clear();
                    this.publishedCache.RemoveComponent(entity);
                }
                
                foreach (var entity in this.filterNextFrame) {
                    this.publishedCache.SetComponent(entity);
                    this.nextFrameCache.RemoveComponent(entity);
                    
                    ref var evnt = ref this.eventsCache.GetComponent(entity);
                    while (evnt.NewData.Count > 0) {
                        evnt.Data.Push(evnt.NewData.Dequeue());
                    }
                }
            }

            public override void Dispose() {
                initialized.Unset(this.worldId);
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