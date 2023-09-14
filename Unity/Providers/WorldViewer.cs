namespace Scellecs.Morpeh.Providers {
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

#if UNITY_EDITOR
    [HideMonoScript]
#endif
    public class WorldViewer : MonoBehaviour {
      
#if UNITY_EDITOR
        public World World {
            get {
                if (this.world == null) {
                    this.world = World.Default;
                }
                return this.world;
            }
            set => this.world = value;
        }
        
        private string GetWorldTitle() => this.World?.GetFriendlyName() ?? "World";

        [DisableContextMenu]
        [PropertySpace]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [Title("$GetWorldTitle")]
        [Searchable]
        private List<EntityView> Entities {
            get {
                var w = this.World;
                if (Application.isPlaying && w != null) {
                    if (w.entitiesCount != this.entityViews.Count) {
                        this.entityViews.Clear();
                        for (int i = 0, length = w.entitiesLength; i < length; i++) {
                            var entity = w.entities[i];
                            if (entity != null) {
                                var view = new EntityView {ID = entity.entityId.id, entityViewer = {getter = () => entity}};
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
        private          World            world;

        [DisableContextMenu]
        [Serializable]
        protected internal class EntityView {
            [ReadOnly]
            public int ID;
            
            [ShowInInspector]
            [InlineProperty]
            [HideReferenceObjectPicker]
            [HideLabel]
            internal Editor.EntityViewer entityViewer = new Editor.EntityViewer();
        }
#endif
    }
}