namespace Morpeh.Globals {
    using System;
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
    public class Singleton : ScriptableObject, IDisposable {
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

        [CanBeNull]
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
            this.entityViewer.getter = () => this.internalEntity;
#endif
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += this.OnEditorApplicationOnplayModeStateChanged;
#else
            CheckIsInitialized();
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
        
        [Serializable]
        private struct SingletonMarker : IComponent { }
    }
}