namespace Morpeh {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Utils;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR && ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class WorldViewer : MonoBehaviour {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [DisableContextMenu]
        [PropertySpace]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [HideReferenceObjectPickerAttribute]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private List<EntityView> Entities {
            get {
                if (Application.isPlaying) {
                    if (World.Default.entitiesCount != this.entityViews.Count) {
                        this.entityViews.Clear();
                        for (int i = 0, length = World.Default.entitiesLength; i < length; i++) {
                            var entity = World.Default.entities[i];
                            if (entity != null) {
                                var view = new EntityView {entity = entity};
                                this.entityViews.Add(view);
                            }
                        }
                    }
                }

                return this.entityViews;
            }
            set { }
        }

        private readonly List<EntityView> entityViews = new List<EntityView>();

        [DisableContextMenu]
        [Serializable]
        protected internal class EntityView {
            internal Entity entity;

            [ShowInInspector]
            private int ID => this.entity.internalID;

            [DisableContextMenu]
            [PropertySpace]
            [ShowInInspector]
            [HideReferenceObjectPickerAttribute]
            [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
            private List<ComponentView> Components {
                get {
                    if (this.entity != null) {
                        this.componentViews.Clear();
                        foreach (var slotIndex in this.entity.componentsIds) {
                            var slot = this.entity.componentsIds.slots[slotIndex];
                            var data = this.entity.componentsIds.data[slotIndex];
                            var view = new ComponentView {
                                debugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[slot.key],
                                id        = data,
                                world     = this.entity.world
                            };
                            this.componentViews.Add(view);
                        }
                    }

                    return this.componentViews;
                }
                set { }
            }

            private List<ComponentView> componentViews = new List<ComponentView>();


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

                internal int id;

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

                        return this.debugInfo.getBoxed(this.world, this.id);
                    }
                    set {
                        if (this.debugInfo.typeInfo.isMarker) {
                            return;
                        }

                        this.debugInfo.setBoxed(this.world, this.id, value);
                    }
                }
            }
        }
#endif
    }
}