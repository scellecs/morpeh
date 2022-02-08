#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System;
    using Morpeh;
    using Unity.Collections;

    public struct NativeFilter<TNative0> : IDisposable
        where TNative0 : unmanaged, IComponent {
        [ReadOnly]
        public int length;
        
        public NativeComponents<TNative0> components0;

        public void Dispose() {
            this.components0.Dispose();
        }
    }

    public struct NativeFilter<TNative0, TNative1> : IDisposable
        where TNative0 : unmanaged, IComponent
        where TNative1 : unmanaged, IComponent {
        [ReadOnly]
        public int length;
        
        public NativeComponents<TNative0> components0;
        public NativeComponents<TNative1> components1;

        public void Dispose() {
            this.components0.Dispose();
            this.components1.Dispose();
        }
    }

    public struct NativeFilter<TNative0, TNative1, TNative2> : IDisposable
        where TNative0 : unmanaged, IComponent
        where TNative1 : unmanaged, IComponent
        where TNative2 : unmanaged, IComponent {
        [ReadOnly]
        public int length;
        
        public NativeComponents<TNative0> components0;
        public NativeComponents<TNative1> components1;
        public NativeComponents<TNative2> components2;

        public void Dispose() {
            this.components0.Dispose();
            this.components1.Dispose();
            this.components2.Dispose();
        }
    }
}
#endif