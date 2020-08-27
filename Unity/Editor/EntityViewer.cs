#if UNITY_EDITOR && ODIN_INSPECTOR
namespace Morpeh.Editor {
    using System;
    using System.Collections.Generic;
    using Collections;
    using Morpeh;
    using Sirenix.OdinInspector;

    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    [HideLabel]
    internal class EntityViewer {
        internal Func<Entity> getter = () => null;
        private  Entity       entity => this.getter();

        private readonly List<ComponentView> componentViews = new List<ComponentView>();

        [DisableContextMenu]
        [PropertySpace]
        [ShowInInspector]
        [HideReferenceObjectPickerAttribute]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private List<ComponentView> ComponentsOnEntity {
            get {
                if (this.entity != null) {
                    this.componentViews.Clear();
                    foreach (var slotIndex in this.entity.componentsIds) {
                        var slot = this.entity.componentsIds.GetKeyByIndex(slotIndex);
                        var data = this.entity.componentsIds.GetValueByIndex(slotIndex);
                        var view = new ComponentView {
                            debugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[slot],
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


        [PropertyTooltip("$" + nameof(FullName))]
        [Serializable]
        private struct ComponentView {
            internal CommonCacheTypeIdentifier.DebugInfo debugInfo;

            internal World world;

            internal bool   IsMarker => this.debugInfo.typeInfo.isMarker;
            internal string FullName => this.debugInfo.type.FullName;

            [ShowIf("$" + nameof(IsMarker))]
            [HideLabel]
            [DisplayAsString(false)]
            [ShowInInspector]
            internal string TypeName => this.debugInfo.type.Name;

            internal int id;

            [DisableContextMenu]
            [HideIf("$" + nameof(IsMarker))]
            [LabelText("$" + nameof(TypeName))]
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

    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    [HideLabel]
    [Title("","Debug Info", HorizontalLine = true)]
    internal class EntityViewerWithHeader : EntityViewer {
        
    }
}
#endif