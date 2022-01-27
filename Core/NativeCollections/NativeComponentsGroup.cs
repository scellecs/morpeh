﻿#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections {
    using System;
    using Morpeh;
    using Sirenix.OdinInspector;
    using Unity.Collections.LowLevel.Unsafe;

    [NativeContainer]
    public struct NativeComponentsGroup<TNative0> : IDisposable
        where TNative0 : unmanaged, IComponent {
        [ReadOnly]
        public int length;
        
        public NativeComponents<TNative0> components0;

        public void Dispose() {
            this.components0.Dispose();
        }
    }

    [NativeContainer]
    public struct NativeComponentsGroup<TNative0, TNative1> : IDisposable
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

    [NativeContainer]
    public struct NativeComponentsGroup<TNative0, TNative1, TNative2> : IDisposable
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