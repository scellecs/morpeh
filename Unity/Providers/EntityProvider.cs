namespace Morpeh {
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(EntityProvider))]
    public class EntityProvider : MonoBehaviour {
        private static IntHashMap<MapItem> map = new IntHashMap<MapItem>();
        private struct MapItem {
            public Entity entity;
            public int    refCounter;
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        private int EntityID => this.Entity != null ? this.cachedEntity.ID.id : -1;

        private Entity cachedEntity;

        [CanBeNull]
        public Entity Entity {
            get {
                if (this.IsPrefab()) {
                    return default;
                }

                if (!Application.isPlaying) {
                    return default;
                }

                if (this.cachedEntity.IsNullOrDisposed()) {
                    this.cachedEntity = null;
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

            if (this.cachedEntity.IsNullOrDisposed()) {
                var instanceId = this.gameObject.GetInstanceID();
                if (map.TryGetValue(instanceId, out var item)) {
                    if (item.entity.IsNullOrDisposed()) {
                        this.cachedEntity = item.entity = World.Default.CreateEntity();
                        item.refCounter   = 1;
                    }
                    else {
                        this.cachedEntity = item.entity;
                        item.refCounter++;
                    }
                    map.Set(instanceId, item, out _);
                }
                else {
                    this.cachedEntity = item.entity = World.Default.CreateEntity();
                    item.refCounter   = 1;
                    map.Add(instanceId, item, out _);
                }
            }

            this.PreInitialize(this.cachedEntity);
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

        protected virtual void PreInitialize(Entity entity) {
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
