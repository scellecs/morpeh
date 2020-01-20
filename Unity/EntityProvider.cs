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
        private Entity entity;

        [CanBeNull]
        public IEntity Entity => this.entity;

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
                                info = CommonCacheTypeIdentifier.editorTypeAssociation[i],
                                id   = this.entity.GetComponentId(i)
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

        private FastBitMask         lastMask       = FastBitMask.None;
        private List<ComponentView> componentViews = new List<ComponentView>();

        
        [PropertyTooltip("$FullName")]
        [Serializable]
        private struct ComponentView {
            internal CommonCacheTypeIdentifier.DebugInfo info;
            
            internal bool   IsMarker => this.info.Info.isMarker;
            internal string FullName => this.info.Type.FullName;

            [ShowIf("$IsMarker")]
            [HideLabel]
            [DisplayAsString(false)]
            [ShowInInspector]
            internal string TypeName => this.info.Type.Name;
            internal int id;

            [DisableContextMenu]
            [HideIf("$IsMarker")]
            [LabelText("$TypeName")]
            [ShowInInspector]
            [HideReferenceObjectPickerAttribute]
            public object Data {
                get {
                    if (this.info.Info.isMarker) {
                        return null;
                    }

                    return this.info.GetBoxed(this.id);
                }
                set {
                    if (this.info.Info.isMarker) {
                        return;
                    }

                    this.info.SetBoxed(this.id, value);
                }
            }
            
        }

#endif
        private protected virtual void Awake() {
            if (this.entity == null) {
                var ent = World.Default.CreateEntityInternal(out _);
                foreach (var monoProvider in this.GetComponents<EntityProvider>()) {
                    monoProvider.entity = ent;
                }
            }

            this.PreInitialize();
            this.Initialize();
        }

        protected virtual void OnDestroy() {
            if (this.entity != null) {
                World.Default.RemoveEntity(this.entity);
                foreach (var monoProvider in this.GetComponents<EntityProvider>()) {
                    monoProvider.entity = null;
                }
            }
        }

        protected virtual void PreInitialize() {
        }

        protected virtual void Initialize() {
        }
    }
}