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
        private Entity entity {
            get {
                if (World.Default != null && this.entityID >= 0 && World.Default.entitiesLength > this.entityID) {
                    return World.Default.entities[this.entityID];
                }

                return null;
            }
        }

        [CanBeNull]
        public IEntity Entity => this.IsPrefab() ? null : Application.isPlaying ? this.entity : null;
#if UNITY_EDITOR && ODIN_INSPECTOR
        private bool isNotEntityProvider => this.GetType() != typeof(EntityProvider);

        [HideIf("$isNotEntityProvider")]
        [DisableContextMenu]
        [PropertySpace]
        [ShowInInspector]
        [HideReferenceObjectPickerAttribute]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private List<ComponentView> ComponentsOnEntity {
            get {
                if (this.entity != null) {
                    this.componentViews.Clear();
                    for (int i = 0, length = this.entity.componentsDoubleCount; i < length; i+=2) {
                        if (this.entity.components[i] == -1) {
                            continue;
                        }
                        var view = new ComponentView {
                            debugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[this.entity.components[i]],
                            ID        = this.entity.components[i + 1],
                            world     = this.entity.World
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
        private protected virtual void Start() {
            if (!Application.isPlaying) {
                return;
            }

            if (this.entityID < 0) {
                var others = this.GetComponents<EntityProvider>();
                foreach (var monoProvider in others) {
                    if (monoProvider.entityID >= 0) {
                        this.entityID = monoProvider.entityID;
                        break;
                    }
                }

                if (this.entityID < 0) {
                    World.Default.CreateEntityInternal(out this.entityID);
                    foreach (var monoProvider in others) {
                        monoProvider.entityID = this.entityID;
                    }
                }
            }

            this.PreInitialize();
            this.Initialize();
        }

        protected virtual void OnDestroy() {
            if (this.entity != null) {
                World.Default?.RemoveEntity(this.entity);
            }
        }

        private bool IsPrefab() => this.gameObject.scene.name == null;

        protected virtual void PreInitialize() {
        }

        protected virtual void Initialize() {
        }
    }
}