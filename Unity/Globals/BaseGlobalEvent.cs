namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using Collections;
    using ECS;
    using UnityEngine;
#if ODIN_INSPECTOR
#endif
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class BaseGlobalEvent<TData> : BaseGlobal {
#if UNITY_EDITOR
        public override Type GetValueType() => typeof(TData);
#endif

        private ComponentsCache<GlobalEventComponent<TData>> dataCache;
        private ComponentsCache<GlobalEventNextFrame>        nextFrameCache;

        public Stack<TData> BatchedChanges {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                ref var component = ref this.dataCache.GetComponent(this.InternalEntity);
                return component.Data;
            }
        }

        public sealed override string LastToString() => this.Serialize(this.BatchedChanges.Peek());
        public abstract string Serialize(TData data);
        public abstract TData  Deserialize(string serializedData);

        protected override bool CheckIsInitialized() {
            var world = World.Default;
            
            var check = base.CheckIsInitialized();

            if (check) {
                var markerCache = world.GetCache<GlobalEventMarker>();
                markerCache.AddComponent(this.internalEntity);
                
                this.dataCache = world.GetCache<GlobalEventComponent<TData>>();
                this.dataCache.SetComponent(this.internalEntity, new GlobalEventComponent<TData> {
                    Action  = null,
                    Data    = new Stack<TData>(),
                    NewData = new Queue<TData>()
                });
                
                var lastStringCache = world.GetCache<GlobalEventLastToString>();
                lastStringCache.SetComponent(this.internalEntity, new GlobalEventLastToString {
                    LastToString = this.LastToString
                });

                this.nextFrameCache = world.GetCache<GlobalEventNextFrame>();
            }

            if (GlobalEventComponentUpdater<TData>.initialized.Get(world.identifier) == false) {
                var updater = new GlobalEventComponentUpdater<TData>();
                updater.Awake(world);
                if (GlobalEventComponentUpdater.updaters.TryGetValue(world.identifier, out var updaters)) {
                    updaters.Add(updater);
                }
                else {
                    GlobalEventComponentUpdater.updaters.Add(world.identifier, new List<GlobalEventComponentUpdater> {updater});
                }
            }

            return check;
        }

        public virtual void Publish(TData data) {
            this.CheckIsInitialized();
            ref var component = ref this.dataCache.GetComponent(this.InternalEntity);
            component.NewData.Enqueue(data);
            this.nextFrameCache.SetComponent(this.InternalEntity);
        }
        
        [Obsolete("Use Publish Instead")]
        public virtual void NextFrame(TData data) {
            this.Publish(data);
        }

        public IDisposable Subscribe(Action<IEnumerable<TData>> callback) {
            this.CheckIsInitialized();
            ref var component = ref this.dataCache.GetComponent(this.InternalEntity);
            component.Action = (Action<IEnumerable<TData>>) Delegate.Combine(component.Action, callback);

            var ent = this.InternalEntity;
            return new Unsubscriber(() => {
                if (ent.IsNullOrDisposed()) {
                    return;
                }

                ref var comp = ref this.dataCache.GetComponent(ent);
                comp.Action = (Action<IEnumerable<TData>>) Delegate.Remove(comp.Action, callback);
            });
        }
    }
}