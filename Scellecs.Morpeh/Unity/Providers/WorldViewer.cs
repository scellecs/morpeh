namespace Scellecs.Morpeh.Providers {
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using Scellecs.Morpeh.Collections;

#if UNITY_EDITOR
    [HideMonoScript]
#endif
    public class WorldViewer : MonoBehaviour {
#if UNITY_EDITOR
        internal static LongHashMap<WeakReference<string>> entityNames = new LongHashMap<WeakReference<string>>();
        
        public World World {
            get {
                if (this.world == null) {
                    this.world = World.Default;
                }
                return this.world;
            }
            set => this.world = value;
        }
        
        private string GetWorldTitle() => "World";

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
                    var entitiesCount = w.entitiesCount;
                    
                    // TODO: May return an incorrect list of entities if entities have changed but their count has not.
                    if (this.entityViews.Count == entitiesCount) {
                        return this.entityViews;
                    }
                    
                    this.entityViews.Clear();
                    
                    for (int i = 0, length = w.entities.Length; i < length; i++) {
                        ref var entityData = ref w.entities[i];
                        
                        if (entityData.currentArchetype == null) {
                            continue;
                        }
                        
                        var entity = world.GetEntityAtIndex(i);
                        
                        this.entityViews.Add(new EntityView {
                            Name = GetCachedEntityName(entity),
                            entityViewer = {
                                world = w,
                                entity = entity,
                            },
                        });
                        
                        if (this.entityViews.Count >= entitiesCount) {
                            break;
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
            [ShowInInspector]
            [InlineProperty]
            [HideLabel]
            public string Name;
            
            [ShowInInspector]
            [InlineProperty]
            [HideReferenceObjectPicker]
            [HideLabel]
            internal Editor.EntityViewer entityViewer = new Editor.EntityViewer();
        }
        
        internal static string GetCachedEntityName(Entity entity) {
            if (entityNames.TryGetValue(entity.value, out var weakReference) && weakReference.TryGetTarget(out var name)) {
                return name;
            }

            var entityStr = entity.ToString();
            entityNames.Set(entity.value, new WeakReference<string>(entityStr), out _);
            return entityStr;
        }
#endif
    }
}