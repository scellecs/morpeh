namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using ECS;
    using UnityEngine;

    public abstract class BaseGlobalEvent<TData> : ScriptableObject, IDisposable {
        private Entity internalEntity;

        public IEntity Entity {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                return this.internalEntity;
            }
        }

        public bool IsPublished {
            get {
#if UNITY_EDITOR 
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                return this.internalEntity.Has<GlobalEventPublished>();
            }
        }

        public List<TData> BatchedChanges {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                ref var component = ref this.internalEntity.GetComponent<GlobalEventComponent<TData>>();
                return component.Data;
            }
        }


        protected virtual void OnEnable() {
            this.internalEntity = null;
        }

        protected virtual void OnDisable() {
            this.internalEntity = null;
        }

        protected void CheckIsInitialized() {
            if (this.internalEntity == null) {
                this.internalEntity = World.Default.CreateEntityInternal();
                this.internalEntity.AddComponent<GlobalEventMarker>();
                this.internalEntity.SetComponent(new GlobalEventComponent<TData> {
                    Action = null,
                    Data   = new List<TData>()
                });

                if (!GlobalEventComponent<TData>.Initialized) {
                    GlobalEventComponentUpdater.Updaters.Add(new GlobalEventComponentUpdater<TData>(this.internalEntity.World.Filter));
                    GlobalEventComponent<TData>.Initialized = true;
                }
            }
        }

        public void Publish(TData data) {
            this.CheckIsInitialized();
            ref var component = ref this.internalEntity.GetComponent<GlobalEventComponent<TData>>(out _);
            component.Data.Add(data);
            this.internalEntity.SetComponent(new GlobalEventPublished());
        }
        
        public void NextFrame(TData data) {
            this.CheckIsInitialized();
            ref var component = ref this.internalEntity.GetComponent<GlobalEventComponent<TData>>(out _);
            component.Data.Add(data);
            this.internalEntity.SetComponent(new GlobalEventNextFrame());
        }

        public IDisposable Subscribe(Action<IEnumerable<TData>> callback) {
            this.CheckIsInitialized();
            ref var component = ref this.internalEntity.GetComponent<GlobalEventComponent<TData>>(out _);
            component.Action = (Action<IEnumerable<TData>>) Delegate.Combine(component.Action, callback);

            var ent = this.internalEntity;
            return new Unsubscriber(() => {
                if (ent == null) {
                    return;
                }

                ref var comp = ref ent.GetComponent<GlobalEventComponent<TData>>(out _);
                comp.Action = (Action<IEnumerable<TData>>) Delegate.Remove(comp.Action, callback);
            });
        }

        public static implicit operator bool(BaseGlobalEvent<TData> exists) => exists.IsPublished;

        private class Unsubscriber : IDisposable {
            private readonly Action unsubscribe;
            public Unsubscriber(Action unsubscribe) => this.unsubscribe = unsubscribe;
            public void Dispose() => this.unsubscribe();
        }

        public void Dispose() {
            if (this.internalEntity != null) {
                this.internalEntity.Dispose();
                World.Default.RemoveEntity(this.internalEntity);
            }
        }

        private void OnDestroy() {
            this.Dispose();
        }
    }
}