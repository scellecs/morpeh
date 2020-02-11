namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using ECS;
    using UnityEditor;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using Unity.IL2CPP.CompilerServices;


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class BaseGlobalEvent<TData> : ScriptableObject, IDisposable {
        [SerializeField]
#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        private int internalEntityID = -1;

        private Entity InternalEntity => World.Default.Entities[this.internalEntityID];

        public IEntity Entity {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                return this.InternalEntity;
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
                return this.InternalEntity.Has<GlobalEventPublished>();
            }
        }

        public Stack<TData> BatchedChanges {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                ref var component = ref this.InternalEntity.GetComponent<GlobalEventComponent<TData>>();
                return component.Data;
            }
        }
        
        internal virtual void OnEnable() {
            this.internalEntityID = -1;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnEditorApplicationOnplayModeStateChanged;
#endif
        }
#if UNITY_EDITOR
        internal virtual void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredEditMode) {
                this.internalEntityID = -1;
            }
        }
#endif
        protected void CheckIsInitialized() {
            if (this.internalEntityID < 0) {
                var ent = World.Default.CreateEntityInternal(out this.internalEntityID);

                ent.AddComponent<GlobalEventMarker>();
                ent.SetComponent(new GlobalEventComponent<TData> {
                    Action = null,
                    Data   = new Stack<TData>()
                });
            }

            if (!GlobalEventComponent<TData>.Initialized) {
                GlobalEventComponentUpdater.Updaters.Add(new GlobalEventComponentUpdater<TData>());
                GlobalEventComponent<TData>.Initialized = true;
            }
        }


        public void Publish(TData data) {
            this.CheckIsInitialized();
            ref var component = ref this.InternalEntity.GetComponent<GlobalEventComponent<TData>>(out _);
            component.Data.Push(data);
            this.InternalEntity.SetComponent(new GlobalEventPublished());
        }

        public void NextFrame(TData data) {
            this.CheckIsInitialized();
            ref var component = ref this.InternalEntity.GetComponent<GlobalEventComponent<TData>>(out _);
            component.Data.Push(data);
            this.InternalEntity.SetComponent(new GlobalEventNextFrame());
        }

        public IDisposable Subscribe(Action<IEnumerable<TData>> callback) {
            this.CheckIsInitialized();
            ref var component = ref this.InternalEntity.GetComponent<GlobalEventComponent<TData>>(out _);
            component.Action = (Action<IEnumerable<TData>>) Delegate.Combine(component.Action, callback);

            var ent = this.InternalEntity;
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

        public virtual void Dispose() {
            if (this.internalEntityID != -1) {
                this.InternalEntity.Dispose();
                World.Default.RemoveEntity(this.InternalEntity);
                this.internalEntityID = -1;
            }
        }

        private void OnDestroy() {
            this.Dispose();
        }
    }
}