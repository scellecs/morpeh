namespace Morpeh {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
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
                                var view = new EntityView {ID = entity.internalID, entityViewer = {getter = () => entity}};
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
            [ReadOnly]
            public int ID;
            
            [ShowInInspector]
            internal Editor.EntityViewer entityViewer = new Editor.EntityViewer();
        }
#endif
    }
}