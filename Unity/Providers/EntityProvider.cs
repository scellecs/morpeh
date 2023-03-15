namespace Scellecs.Morpeh.Providers {
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(EntityProvider))]
    public class EntityProvider : MonoBehaviour {
        public static IntHashMap<MapItem> map = new IntHashMap<MapItem>();
        public struct MapItem {
            public Entity entity;
            public int    refCounter;
        }

#if UNITY_EDITOR
        [ShowInInspector]
        [PropertyOrder(-1)]
        [ReadOnly]
#endif
        private int EntityID => this.cachedEntity.IsNullOrDisposed() == false ? this.cachedEntity.ID.id : -1;

        protected internal Entity cachedEntity;

        [CanBeNull]
        public Entity Entity {
            get {
                if (this.IsEditmodeOrPrefab()) {
                    return default;
                }

                return this.cachedEntity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsEditmodeOrPrefab() {
            if (World.Default == null) {
                return true;
            }
            if (Application.isPlaying == false) {
                return true;
            }
            if (this.IsPrefab() == true) {
                return true;
            }
            
            return false;
        }

        protected void CheckEntityInitialization() {
            if (this.cachedEntity.IsNullOrDisposed()) {
                var instanceId = this.gameObject.GetInstanceID();
                if (map.TryGetValue(instanceId, out var item)) {
                    if (item.entity.IsNullOrDisposed()) {
                        this.cachedEntity = item.entity = World.Default.CreateEntity();
                    }
                    else {
                        this.cachedEntity = item.entity;
                    }
                    item.refCounter++;
                    map.Set(instanceId, item, out _);
                }
                else {
                    this.cachedEntity = item.entity = World.Default.CreateEntity();
                    item.refCounter   = 1;
                    map.Add(instanceId, item, out _);
                }
            }
        }

        protected virtual void OnEnable() {
#if UNITY_EDITOR
            this.entityViewer.getter = () => this.Entity;
#endif
            if (this.IsEditmodeOrPrefab()) {
                return;
            }
            
            this.CheckEntityInitialization();
            
            this.PreInitialize();
            this.Initialize();
        }

        protected virtual void OnDisable() {
            if (this.IsEditmodeOrPrefab()) {
                return;
            }
            
            this.PreDeinitialize();
            this.Deinitialize();
            
            var instanceId = this.gameObject.GetInstanceID();
            if (map.TryGetValue(instanceId, out var item)) {
                if (--item.refCounter <= 0) {
                    map.Remove(instanceId, out _);
                }
            }
        }

        private bool IsPrefab() => this.gameObject.scene.rootCount == 0;

        protected virtual void PreInitialize() {
        }

        protected virtual void Initialize() {
        }
        
        protected virtual void PreDeinitialize() {
        }

        protected virtual void Deinitialize() {
        }

#if UNITY_EDITOR
        private bool IsNotEntityProvider {
            get {
                var type = this.GetType();
                return type != typeof(EntityProvider) && type != typeof(UniversalProvider);
            }
        }

        [HideIf("$" + nameof(IsNotEntityProvider))]
        [PropertyOrder(100)]
        [ShowInInspector]
        [InlineProperty]
        [HideReferenceObjectPicker]
        [HideLabel]
        [Title("","Debug Info", HorizontalLine = true)]
        private Editor.EntityViewer entityViewer = new Editor.EntityViewer();
#endif
    }
}
