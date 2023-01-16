namespace Scellecs.Morpeh.Providers {
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

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        private int EntityID => this.cachedEntity.IsNullOrDisposed() == false ? this.cachedEntity.ID.id : -1;

        private Entity cachedEntity;

        [CanBeNull]
        public Entity Entity {
            get {
                if (World.Default == null) {
                    return default;
                }
                
                if (this.IsPrefab()) {
                    return default;
                }

                if (!Application.isPlaying) {
                    return default;
                }

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

                return this.cachedEntity;
            }
        }

        private protected virtual void OnEnable() {
#if UNITY_EDITOR && ODIN_INSPECTOR
            this.entityViewer.getter = () => this.Entity;
#endif
            if (!Application.isPlaying) {
                return;
            }

            this.PreInitialize();
            this.Initialize();
        }

        protected virtual void OnDisable() {
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

#if UNITY_EDITOR && ODIN_INSPECTOR
        private bool IsNotEntityProvider {
            get {
                var type = this.GetType();
                return type != typeof(EntityProvider) && type != typeof(UniversalProvider);
            }
        }

        [HideIf("$" + nameof(IsNotEntityProvider))]
        [ShowInInspector]
        [PropertyOrder(100)]
        private Editor.EntityViewerWithHeader entityViewer = new Editor.EntityViewerWithHeader();
#endif
    }
}
