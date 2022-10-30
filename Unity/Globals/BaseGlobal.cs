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
    public abstract class BaseGlobal : BaseSingleton, IDisposable {
        private ComponentsCache<GlobalEventPublished> publishedCache;

        public bool IsPublished {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return default;
                }
                this.CheckIsInitialized();
                return this.publishedCache.Has(this.InternalEntity);
#else
                return this.publishedCache.Has(this.internalEntity);
#endif
            }
        }
        
#if UNITY_EDITOR
        public abstract Type GetValueType();
#endif
        public abstract string LastToString();

        protected override bool CheckIsInitialized() {
            if (base.CheckIsInitialized()) {
                this.publishedCache = this.internalEntity.world.GetCache<GlobalEventPublished>();
                
                return true;
            }
            return false;
        }

        public static implicit operator bool(BaseGlobal exists) => exists != null && exists.IsPublished;

        protected class Unsubscriber : IDisposable {
            private readonly Action unsubscribe;
            public Unsubscriber(Action unsubscribe) => this.unsubscribe = unsubscribe;
            public void Dispose() => this.unsubscribe();
        }
    }
}