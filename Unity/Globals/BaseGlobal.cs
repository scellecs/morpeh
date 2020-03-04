namespace Morpeh.Globals {
    using System;
    using ECS;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEditor;
    using UnityEngine;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class BaseGlobal : ScriptableObject, IDisposable {
        [SerializeField]
#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        private protected int internalEntityID = -1;

        private protected Entity InternalEntity => World.Default.entities[this.internalEntityID];

        public IEntity Entity {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                return this.InternalEntity;
            }
        }

        public bool IsPublished {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
#endif
                this.CheckIsInitialized();
                return this.InternalEntity.Has<GlobalEventPublished>();
            }
        }
        
#if UNITY_EDITOR
        public abstract Type GetValueType();
#endif
        internal virtual void OnEnable() {
            this.internalEntityID = -1;
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
            }
        }
#endif

        private protected abstract void CheckIsInitialized();

        public static implicit operator bool(BaseGlobal exists) => exists.IsPublished;

        private protected class Unsubscriber : IDisposable {
            private readonly Action unsubscribe;
            public Unsubscriber(Action unsubscribe) => this.unsubscribe = unsubscribe;
            public void Dispose() => this.unsubscribe();
        }

        public virtual void Dispose() {
            if (this.internalEntityID != -1) {
                this.InternalEntity.Dispose();
                World.Default.RemoveEntity(this.InternalEntity);
                this.internalEntityID = -1;
            }
        }

        private void OnDestroy() {
            this.Dispose();
        }
    }
}