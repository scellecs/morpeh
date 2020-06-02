namespace Morpeh.Globals {
    using System;
    using ECS;
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
    public abstract class BaseGlobal : Singleton, IDisposable {
        internal bool isPublished;

        public bool IsPublished {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
                this.CheckIsInitialized();
#endif
                return this.isPublished;
            }
        }
        
#if UNITY_EDITOR
        public abstract Type GetValueType();
#endif
        internal override void OnEnable() {
            base.OnEnable();
#if UNITY_EDITOR
            if (Application.isPlaying) {
                this.isPublished = this.InternalEntity.Has<GlobalEventPublished>();
            }
#endif
        }

        public abstract string LastToString();

        public static implicit operator bool(BaseGlobal exists) => exists != null && exists.IsPublished;

        private protected class Unsubscriber : IDisposable {
            private readonly Action unsubscribe;
            public Unsubscriber(Action unsubscribe) => this.unsubscribe = unsubscribe;
            public void Dispose() => this.unsubscribe();
        }
    }
}