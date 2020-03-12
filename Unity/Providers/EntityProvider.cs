namespace Morpeh {
    using JetBrains.Annotations;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using Utils;

#endif

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
                    return null;
                }

                if (!Application.isPlaying) {
                    return null;
                }
                
                if (this.cachedEntity == null) {
                    if (World.Default != null && this.entityID >= 0 && World.Default.entitiesLength > this.entityID) {
                        this.cachedEntity = World.Default.entities[this.entityID];
                    }
                }
                else if (this.cachedEntity != null && this.cachedEntity.IsDisposed()) {
                    this.cachedEntity = null;
                }
                
                return this.cachedEntity;
            }
        }

        private Entity cachedEntity;

        [CanBeNull]
        public IEntity Entity => this.InternalEntity;

        private protected virtual void OnEnable() {
            if (!Application.isPlaying) {
                return;
            }

            if (this.entityID < 0) {
                var others = this.GetComponents<EntityProvider>();
                foreach (var entityProvider in others) {
                    if (entityProvider.entityID >= 0) {
                        this.entityID = entityProvider.entityID;
                        this.cachedEntity = entityProvider.cachedEntity;
                        break;
                    }
                }

                if (this.entityID < 0 || this.InternalEntity == null) {
                    this.cachedEntity = World.Default.CreateEntityInternal(out this.entityID);
                    foreach (var entityProvider in others) {
                        entityProvider.entityID = this.entityID;
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

        [System.Obsolete]
        protected virtual void OnDestroy() {
            
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
        [DisableContextMenu]
        [PropertySpace]
        [ShowInInspector]
        [HideReferenceObjectPickerAttribute]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private List<ComponentView> ComponentsOnEntity {
            get {
                this.componentViews.Clear();
                if (this.InternalEntity != null) {
                    for (int i = 0, length = this.InternalEntity.componentsDoubleCapacity; i < length; i += 2) {
                        if (this.InternalEntity.components[i] == -1) {
                            continue;
                        }

                        var view = new ComponentView {
                            debugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[this.InternalEntity.components[i]],
                            ID        = this.InternalEntity.components[i + 1],
                            world     = this.InternalEntity.World
                        };
                        this.componentViews.Add(view);
                    }
                }

                return this.componentViews;
            }
            set { }
        }

        private readonly List<ComponentView> componentViews = new List<ComponentView>();


        [PropertyTooltip("$FullName")]
        [Serializable]
        private struct ComponentView {
            internal CommonCacheTypeIdentifier.DebugInfo debugInfo;
            internal World                               world;

            internal bool   IsMarker => this.debugInfo.typeInfo.isMarker;
            internal string FullName => this.debugInfo.type.FullName;

            [ShowIf("$IsMarker")]
            [HideLabel]
            [DisplayAsString(false)]
            [ShowInInspector]
            internal string TypeName => this.debugInfo.type.Name;

            internal int ID;

            [DisableContextMenu]
            [HideIf("$IsMarker")]
            [LabelText("$TypeName")]
            [ShowInInspector]
            [HideReferenceObjectPickerAttribute]
            public object Data {
                get {
                    if (this.debugInfo.typeInfo.isMarker) {
                        return null;
                    }

                    return this.debugInfo.getBoxed(this.world, this.ID);
                }
                set {
                    if (this.debugInfo.typeInfo.isMarker) {
                        return;
                    }

                    this.debugInfo.setBoxed(this.world, this.ID, value);
                }
            }
        }

#endif
    }
}