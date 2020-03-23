namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
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

        private protected override void CheckIsInitialized() {
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
    }
}