#if UNITY_EDITOR && TRI_INSPECTOR
namespace Morpeh.Editor {
    using System;
    using System.Collections.Generic;
    using Collections;
    using Morpeh;
    using UnityEngine;
    using TriInspector;

    [Serializable]
    [InlineProperty]
    internal class EntityViewer {
        internal Func<Entity> getter = () => null;
        private  Entity       entity => this.getter();

        private readonly List<ComponentView> componentViews = new List<ComponentView>();

        [PropertySpace]
        [ShowInInspector]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        private List<ComponentView> ComponentsOnEntity {
            get {
                this.componentViews.Clear();
                if (this.entity != null && this.entity.world != null) {
                    var caches = this.entity.world.caches;
                    foreach (var cacheId in caches) {
                        var cache = ComponentsCache.caches.data[caches.GetValueByIndex(cacheId)];
                        if (cache.Has(this.entity)) {
                            var view = new ComponentView {
                                internalTypeDefinition = CommonTypeIdentifier.intTypeAssociation[cache.typeId],
                                entity                 = this.entity
                            };
                            this.componentViews.Add(view);
                        }
                    }
                }

                return this.componentViews;
            }
            set { }
        }


        [Serializable]
        private struct ComponentView {
            internal CommonTypeIdentifier.InternalTypeDefinition internalTypeDefinition;

            internal bool   IsMarker => this.internalTypeDefinition.typeInfo.isMarker;
            internal string FullName => this.internalTypeDefinition.type.FullName;

            [HideLabel]
            [ShowInInspector]
            internal string Component => this.internalTypeDefinition.type.Name;

            internal Entity entity;

            [HideIf(nameof(IsMarker))]
            [HideLabel]
            [HideReferencePicker]
            [ShowInInspector]
            public object Data {
                get {
                    if (this.internalTypeDefinition.typeInfo.isMarker || Application.isPlaying == false) {
                        return null;
                    }

                    return this.internalTypeDefinition.entityGetComponentBoxed(this.entity);
                }
                set {
                    if (this.internalTypeDefinition.typeInfo.isMarker || Application.isPlaying == false) {
                        return;
                    }

                    this.internalTypeDefinition.entitySetComponentBoxed(this.entity, value);
                }
            }
        }
    }

    [Serializable]
    [InlineProperty]
    internal class EntityViewerWithHeader : EntityViewer {
        
    }
}
#endif