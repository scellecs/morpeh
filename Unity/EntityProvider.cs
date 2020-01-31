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
        [ShowInInspector]
        [ReadOnly]
        private int entityID = -1;

        [CanBeNull]
        private Entity entity {
            get {
                if (World.Default != null && this.entityID >= 0 && World.Default.EntitiesLength > this.entityID) {
                    return World.Default.Entities[this.entityID];
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
                if (this.entity != null && this.lastMask != this.entity.ComponentsMask) {
                    this.componentViews.Clear();
                    for (int i = 0, length = 256; i < length; i++) {
                        if (this.entity.ComponentsMask.GetBit(i)) {
                            var view = new ComponentView {
                                debugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[i],
                                ID        = this.entity.GetComponentId(i),
                                World     = this.entity.World
                            };
                            this.componentViews.Add(view);
                        }
                    }

                    this.lastMask = this.entity.ComponentsMask;
                }

                return this.componentViews;
            }
            set { }
        }

        private          FastBitMask         lastMask       = FastBitMask.None;
        private readonly List<ComponentView> componentViews = new List<ComponentView>();


        [PropertyTooltip("$FullName")]
        [Serializable]
        private struct ComponentView {
            internal CommonCacheTypeIdentifier.DebugInfo debugInfo;
            internal World                               World;

            internal bool   IsMarker => this.debugInfo.TypeInfo.isMarker;
            internal string FullName => this.debugInfo.Type.FullName;

            [ShowIf("$IsMarker")]
            [HideLabel]
            [DisplayAsString(false)]
            [ShowInInspector]
            internal string TypeName => this.debugInfo.Type.Name;

            internal int ID;

            [DisableContextMenu]
            [HideIf("$IsMarker")]
            [LabelText("$TypeName")]
            [ShowInInspector]
            [HideReferenceObjectPickerAttribute]
            public object Data {
                get {
                    if (this.debugInfo.TypeInfo.isMarker) {
                        return null;
                    }

                    return this.debugInfo.GetBoxed(this.World, this.ID);
                }
                set {
                    if (this.debugInfo.TypeInfo.isMarker) {
                        return;
                    }

                    this.debugInfo.SetBoxed(this.World, this.ID, value);
                }
            }
        }

#endif
        private protected virtual void Awake() {
            if (!Application.isPlaying) {
                return;
            }

            if (this.entityID < 0) {
                World.Default.CreateEntityInternal(out this.entityID);
                foreach (var monoProvider in this.GetComponents<EntityProvider>()) {
                    monoProvider.entityID = this.entityID;
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