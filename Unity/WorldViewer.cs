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
                    if (World.Default.EntitiesCount != this.entityViews.Count) {
                        this.entityViews.Clear();
                        for (int i = 0, length = World.Default.EntitiesLength; i < length; i++) {
                            var entity = World.Default.Entities[i];
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
            private int ID => this.entity.InternalID;

            [DisableContextMenu]
            [PropertySpace]
            [ShowInInspector]
            [HideReferenceObjectPickerAttribute]
            [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
            private List<ComponentView> Components {
                get {
                    if (this.entity != null && this.lastMask != this.entity.ComponentsMask) {
                        this.componentViews.Clear();
                        for (int i = 0, length = 256; i < length; i++) {
                            if (this.entity.ComponentsMask.GetBit(i)) {
                                var view = new ComponentView {
                                    DebugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[i],
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

            private FastBitMask         lastMask       = FastBitMask.None;
            private List<ComponentView> componentViews = new List<ComponentView>();


            [Serializable]
            private struct ComponentView {
                internal CommonCacheTypeIdentifier.DebugInfo DebugInfo;
                internal World                               World;


                internal bool   IsMarker => this.DebugInfo.TypeInfo.isMarker;
                internal string FullName => this.DebugInfo.Type.FullName;

                [ShowIf("$IsMarker")]
                [HideLabel]
                [DisplayAsString(false)]
                [ShowInInspector]
                internal string TypeName => this.DebugInfo.Type.Name;

                internal int ID;

                [DisableContextMenu]
                [HideIf("$IsMarker")]
                [LabelText("$TypeName")]
                [ShowInInspector]
                [HideReferenceObjectPickerAttribute]
                public object Data {
                    get {
                        if (this.DebugInfo.TypeInfo.isMarker) {
                            return null;
                        }

                        return this.DebugInfo.GetBoxed(this.World, this.ID);
                    }
                    set {
                        if (this.DebugInfo.TypeInfo.isMarker) {
                            return;
                        }

                        this.DebugInfo.SetBoxed(this.World, this.ID, value);
                    }
                }
            }
        }
#endif
    }
}