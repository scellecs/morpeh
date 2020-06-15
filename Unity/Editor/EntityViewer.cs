#if UNITY_EDITOR && ODIN_INSPECTOR
namespace Morpeh.Editor {
    using System;
    using System.Collections.Generic;
    using Morpeh;
    using Sirenix.OdinInspector;

    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    [HideLabel]
    [Title("","Debug Info", HorizontalLine = true)]
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
                this.componentViews.Clear();
                if (this.entity != null) {
                    for (int i = 0, length = this.entity.componentsDoubleCapacity; i < length; i += 2) {
                        if (this.entity.components[i] == -1) {
                            continue;
                        }

                        var view = new ComponentView {
                            debugInfo = CommonCacheTypeIdentifier.editorTypeAssociation[this.entity.components[i]],
                            ID        = this.entity.components[i + 1],
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

            internal int ID;

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
    }
}
#endif