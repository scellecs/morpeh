namespace Morpeh {
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
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
#endif
        protected int internalEntityID = -1;

        [CanBeNull]
        private Entity InternalEntity {
            get {
                if (this.IsPrefab()) {
                    return default;
                }

                if (!Application.isPlaying) {
                    return default;
                }

                if (this.cachedEntity == null) {
                    if (World.Default != null && this.internalEntityID >= 0 && World.Default.entitiesLength > this.internalEntityID) {
                        this.cachedEntity = World.Default.entities[this.internalEntityID];
                    }
                }
                else if (this.cachedEntity != null && this.cachedEntity.isDisposed) {
                    this.cachedEntity = null;
                    this.internalEntityID     = -1;
                }

                return this.cachedEntity;
            }
        }

        private Entity cachedEntity;

        [CanBeNull]
        public Entity Entity => this.InternalEntity;

        private protected virtual void OnEnable() {
#if UNITY_EDITOR && ODIN_INSPECTOR
            this.entityViewer.getter = () => this.InternalEntity;
#endif
            if (!Application.isPlaying) {
                return;
            }

            if (this.internalEntityID < 0) {
                var others = this.GetComponents<EntityProvider>();
                foreach (var entityProvider in others) {
                    if (entityProvider.internalEntityID >= 0) {
                        this.internalEntityID     = entityProvider.internalEntityID;
                        this.cachedEntity = entityProvider.cachedEntity;
                        break;
                    }
                }
            }

            if (this.InternalEntity == null || this.internalEntityID < 0) {
                var others = this.GetComponents<EntityProvider>();
                this.cachedEntity = World.Default.CreateEntity(out this.internalEntityID);
                foreach (var entityProvider in others) {
                    if (entityProvider.enabled) {
                        entityProvider.internalEntityID     = this.internalEntityID;
                        entityProvider.cachedEntity = this.cachedEntity;
                    }
                }
            }

            this.PreInitialize();
            this.Initialize();
        }

        protected virtual void OnDisable() {
            var others = this.GetComponents<EntityProvider>();
            foreach (var entityProvider in others) {
                entityProvider.CheckEntityIsAlive();
            }
        }

        private void CheckEntityIsAlive() {
            if (this.InternalEntity == null || this.InternalEntity.isDisposed) {
                this.internalEntityID = -1;
            }
        }

        private bool IsPrefab() => this.gameObject.scene.name == null;

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