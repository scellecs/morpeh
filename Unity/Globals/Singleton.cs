namespace Morpeh.Globals {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using Unity.IL2CPP.CompilerServices;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Singleton")]
    public class Singleton : ScriptableObject, IEntity {
        [SerializeField]
#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        private protected int internalEntityID = -1;

        private protected Entity internalEntity;
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [Space]
        private Morpeh.Editor.EntityViewer entityViewer = new Morpeh.Editor.EntityViewer();
#endif

        [CanBeNull]
        private protected Entity InternalEntity {
            get {
                if (this.internalEntity == null) {
                    this.internalEntity = World.Default.entities[this.internalEntityID];
                }

                return this.internalEntity;
            }
        }

        [NotNull]
        public IEntity Entity {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }

                this.CheckIsInitialized();
#endif
                return this.InternalEntity;
            }
        }
        
        internal virtual void OnEnable() {
            this.internalEntity = null;
#if UNITY_EDITOR && ODIN_INSPECTOR
            this.entityViewer = new Morpeh.Editor.EntityViewer {getter = () => this.internalEntity};
#endif
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += this.OnEditorApplicationOnplayModeStateChanged;
            if (Application.isPlaying) {
#endif
                this.CheckIsInitialized();
#if UNITY_EDITOR
            }
#endif
        }
        
#if UNITY_EDITOR
        internal virtual void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredEditMode) {
                this.internalEntityID = -1;
                this.internalEntity   = null;
            }
        }
#endif
        private protected virtual bool CheckIsInitialized() {
            if (this.internalEntityID < 0) {
                this.internalEntity = World.Default.CreateEntityInternal(out this.internalEntityID);
                this.internalEntity.AddComponent<SingletonMarker>();
                return true;
            }

            return false;
        }
        
        public virtual void Dispose() {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= this.OnEditorApplicationOnplayModeStateChanged;
#endif
            if (this.internalEntityID != -1) {
                var entity = this.InternalEntity;
                if (entity != null && !entity.IsDisposed()) {
                    World.Default.RemoveEntity(entity);
                }
                this.internalEntityID = -1;
                this.internalEntity   = null;
            }
        }

        
        private void OnDestroy() {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= this.OnEditorApplicationOnplayModeStateChanged;
#endif
        }
        public int ID => this.Entity.ID;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponent => ref this.Entity.AddComponent<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(out bool exist) where T : struct, IComponent => ref this.Entity.AddComponent<T>(out exist);

        public bool AddComponentFast(in int typeId, in int componentId) => this.Entity.AddComponentFast(typeId, componentId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>() where T : struct, IComponent => ref this.Entity.GetComponent<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>(out bool exist) where T : struct, IComponent => ref this.Entity.GetComponent<T>(out exist);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentFast(in int typeId) => this.Entity.GetComponentFast(typeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(in T value) where T : struct, IComponent => this.Entity.SetComponent(in value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponent => this.Entity.RemoveComponent<T>();

        public bool RemoveComponentFast(int typeId, out int cacheIndex) => this.Entity.RemoveComponentFast(typeId, out cacheIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has<T>() where T : struct, IComponent => this.Entity.Has<T>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDisposed() => this.Entity.IsDisposed();
        
        [Serializable]
        private struct SingletonMarker : IComponent { }
    }
}