#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.Collections {
    using System;
    using Morpeh;
    using Unity.Collections;
    
    public struct NativeFilter<TNative0> : IDisposable
        where TNative0 : unmanaged, IComponent {
        public int                        Length;
        public NativeComponents<TNative0> Components0;

        public void Dispose() {
            this.Components0.Dispose();
        }
    }

    public struct NativeFilter<TNative0, TNative1> : IDisposable
        where TNative0 : unmanaged, IComponent
        where TNative1 : unmanaged, IComponent {
        public int                        Length;
        public NativeComponents<TNative0> Components0;
        public NativeComponents<TNative1> Components1;

        public void Dispose() {
            this.Components0.Dispose();
            this.Components1.Dispose();
        }
    }

    public struct NativeFilter<TNative0, TNative1, TNative2> : IDisposable
        where TNative0 : unmanaged, IComponent
        where TNative1 : unmanaged, IComponent
        where TNative2 : unmanaged, IComponent {
        public int                        Length;
        public NativeComponents<TNative0> Components0;
        public NativeComponents<TNative1> Components1;
        public NativeComponents<TNative2> Components2;
        
        public void Dispose() {
            this.Components0.Dispose();
            this.Components1.Dispose();
            this.Components2.Dispose();
        }
    }
}
#endif