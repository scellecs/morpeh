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
        private int entityID = -1;

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
                    if (World.Default != null && this.entityID >= 0 && World.Default.entitiesLength > this.entityID) {
                        this.cachedEntity = World.Default.entities[this.entityID];
                    }
                }
                else if (this.cachedEntity != null && this.cachedEntity.IsDisposed()) {
                    this.cachedEntity = null;
                    this.entityID     = -1;
                }

                return this.cachedEntity;
            }
        }

        private Entity cachedEntity;

        [CanBeNull]
        public IEntity Entity => this.InternalEntity;

        private protected virtual void OnEnable() {
#if UNITY_EDITOR && ODIN_INSPECTOR
            this.entityViewer.getter = () => this.InternalEntity;
#endif
            if (!Application.isPlaying) {
                return;
            }

            if (this.entityID < 0) {
                var others = this.GetComponents<EntityProvider>();
                foreach (var entityProvider in others) {
                    if (entityProvider.entityID >= 0) {
                        this.entityID     = entityProvider.entityID;
                        this.cachedEntity = entityProvider.cachedEntity;
                        break;
                    }
                }
            }

            if (this.InternalEntity == null || this.entityID < 0) {
                var others = this.GetComponents<EntityProvider>();
                this.cachedEntity = World.Default.CreateEntityInternal(out this.entityID);
                foreach (var entityProvider in others) {
                    entityProvider.entityID     = this.entityID;
                    entityProvider.cachedEntity = this.cachedEntity;
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
            if (this.InternalEntity == null || this.InternalEntity.IsDisposed()) {
                this.entityID = -1;
            }
        }

        private bool IsPrefab() => this.gameObject.scene.name == null;

        protected virtual void PreInitialize() {
        }

        protected virtual void Initialize() {
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private bool IsNotEntityProvider => this.GetType() != typeof(EntityProvider);

        [HideIf("$" + nameof(IsNotEntityProvider))]
        [ShowInInspector]
        private Editor.EntityViewer entityViewer = new Editor.EntityViewer();
#endif
    }
}