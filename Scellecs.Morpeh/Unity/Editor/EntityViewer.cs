#if UNITY_EDITOR
namespace Scellecs.Morpeh.Editor {
    using System;
    using System.Collections.Generic;
    using Scellecs.Morpeh.Collections;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    internal class EntityViewer {
        internal World world;
        internal Entity entity;

        private readonly List<ComponentView> componentViews = new List<ComponentView>();

        [DisableContextMenu]
        [PropertySpace]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private List<ComponentView> ComponentsOnEntity {
            get {
                this.componentViews.Clear();
                
                if (this.world == null || this.world.IsDisposed || !this.world.Has(entity)) {
                    return this.componentViews;
                }

                var archetype = this.world.entities[entity.Id].currentArchetype;
                if (archetype == null) {
                    return this.componentViews;
                }
                
                foreach (var typeId in archetype.components) {
                    var view = new ComponentView {
                        internalTypeDefinition = ExtendedComponentId.Get(this.world.stashes[typeId].Type),
                        entity = this.entity,
                    };
                    this.componentViews.Add(view);
                }
                
                return this.componentViews;
            }
            set { }
        }


        //TODO: impl PropertyTooltip attribute
        //[PropertyTooltip("$" + nameof(FullName))]
        [Serializable]
        private struct ComponentView {
            internal ExtendedComponentId.InternalTypeDefinition internalTypeDefinition;

            internal bool   IsMarker => this.internalTypeDefinition.isMarker;
            internal string FullName => this.internalTypeDefinition.type.FullName;

            [ShowIf("$" + nameof(IsMarker))]
            [HideLabel]
            [DisplayAsString(false)]
            [ShowInInspector]
            internal string TypeName => this.internalTypeDefinition.type.Name;

            internal Entity entity;

            [DisableContextMenu]
            [HideIf("$" + nameof(IsMarker))]
            [LabelText("$" + nameof(TypeName))]
            [ShowInInspector]
            [HideReferenceObjectPicker]
            public object Data {
                get {
                    if (this.internalTypeDefinition.isMarker || Application.isPlaying == false) {
                        return null;
                    }

                    return this.internalTypeDefinition.entityGetComponentBoxed(this.entity);
                }
                set {
                    if (this.internalTypeDefinition.isMarker || Application.isPlaying == false) {
                        return;
                    }

                    this.internalTypeDefinition.entitySetComponentBoxed(this.entity, value);
                }
            }
        }
    }
}
#endif