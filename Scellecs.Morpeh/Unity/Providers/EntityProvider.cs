namespace Scellecs.Morpeh.Providers {
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [AddComponentMenu("ECS/" + nameof(EntityProvider))]
    public class EntityProvider : MonoBehaviour {
#pragma warning disable 0618
#if UNITY_6000_5_OR_NEWER
        public static LongHashMap<MapItem> map = new LongHashMap<MapItem>();
#else
        public static IntHashMap<MapItem> map = new IntHashMap<MapItem>();
#endif
        public struct MapItem {
            public Entity entity;
            public int    refCounter;
        }

#if UNITY_EDITOR
        [ShowInInspector]
        [PropertyOrder(-1)]
        [ReadOnly]
#endif
        private int EntityID => this.cachedEntity.IsNullOrDisposed() == false ? this.cachedEntity.Id : -1;

        protected internal Entity cachedEntity;

        [CanBeNull]
        public Entity Entity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.cachedEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsEditmodeOrPrefab() {
            if (World.Default == null) {
                return true;
            }
            if (Application.isPlaying == false) {
                return true;
            }
            if (this.IsPrefab() == true) {
                return true;
            }
            
            return false;
        }

        protected void CheckEntityInitialization() {
            if (this.cachedEntity.IsNullOrDisposed()) {
                var id = GetGameObjectIdentifier(this.gameObject);
                
                if (map.TryGetValue(id, out var item)) {
                    if (item.entity.IsNullOrDisposed()) {
                        this.cachedEntity = item.entity = World.Default.CreateEntity();
                    }
                    else {
                        this.cachedEntity = item.entity;
                    }
                    item.refCounter++;
                    map.Set(id, item, out _);
                }
                else {
                    this.cachedEntity = item.entity = World.Default.CreateEntity();
                    item.refCounter   = 1;
                    map.Add(id, item, out _);
                }
            }
        }

        protected virtual void OnEnable() {
            if (this.IsEditmodeOrPrefab()) {
                return;
            }
            
            this.CheckEntityInitialization();
            
            this.PreInitialize();
            this.Initialize();
            
#if UNITY_EDITOR
            if (this.entityViewer != null) {
                this.entityViewer.world = World.Default;
                this.entityViewer.entity = this.Entity;
            }
#endif
        }

        protected virtual void OnDisable() {
            if (this.IsEditmodeOrPrefab()) {
                return;
            }
            
            this.PreDeinitialize();
            this.Deinitialize();

            var id = GetGameObjectIdentifier(this.gameObject);
            
            if (map.TryGetValue(id, out var item)) {
                item.refCounter--;
                if (item.refCounter <= 0) {
                    map.Remove(id, out _);
                } 
                else {
                    map.Set(id, item, out _);
                }
            }
            
#if UNITY_EDITOR
            if (this.entityViewer != null) {
                this.entityViewer.world = default;
                this.entityViewer.entity = default;
            }
#endif
        }

        private bool IsPrefab() => this.gameObject.scene.rootCount == 0;

        protected virtual void PreInitialize() {
        }

        protected virtual void Initialize() {
        }
        
        protected virtual void PreDeinitialize() {
        }

        protected virtual void Deinitialize() {
        }
        
#if UNITY_6005_OR_NEWER
        private static long GetGameObjectIdentifier(GameObject obj) {
            return (long)obj.GetEntityId().ToULong();
        }
#else
        private static int GetGameObjectIdentifier(GameObject obj)
        {
#if UNITY_6000_3_OR_NEWER
            return obj.GetEntityId().GetHashCode();
#else
            return obj.GetInstanceID();
#endif
        }
#endif

#if UNITY_EDITOR
        private bool IsNotEntityProvider {
            get {
                var type = this.GetType();
                return type != typeof(EntityProvider);
            }
        }
        
        private Editor.EntityViewer entityViewer;
        
        [HideIf("$IsNotEntityProvider")]
        [PropertyOrder(100)]
        [ShowInInspector]
        [InlineProperty]
        [HideReferenceObjectPicker]
        [HideLabel]
        [Title("","Debug Info", HorizontalLine = true)]
        private Editor.EntityViewer EntityViewer {
            get {
                if (this.entityViewer == null) {
                    this.entityViewer = new Editor.EntityViewer();

                    this.entityViewer.world  = World.Default;
                    this.entityViewer.entity = this.Entity;
                }
                
                return this.entityViewer;
            }
        }
#endif
#pragma warning restore 0618
    }
}
