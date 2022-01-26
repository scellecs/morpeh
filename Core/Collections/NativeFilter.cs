#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.Collections {
    using System;
    using Morpeh;
    using Unity.Collections;

    public struct NativeFilter<TNative0> : IDisposable
        where TNative0 : unmanaged, IComponent {
        public NativeArray<int> Components0Ids;
        public NativeArray<TNative0>  Components0Values;
        
        public void Dispose() {
            this.Components0Ids.Dispose();
            this.Components0Values.Dispose();
        }
    }
    
    public struct NativeFilter<TNative0, TNative1> : IDisposable
        where TNative0 : unmanaged, IComponent
        where TNative1 : unmanaged, IComponent {
        public NativeArray<int> Components0Ids;
        public NativeArray<TNative0>  Components0Values;
        public NativeArray<int> Components1Ids;
        public NativeArray<TNative1>  Components1Values;

        public void Dispose() {
            this.Components0Ids.Dispose();
            this.Components0Values.Dispose();
            this.Components1Ids.Dispose();
            this.Components1Values.Dispose();
        }
    }
    
    public struct NativeFilter<TNative0, TNative1, TNative2> : IDisposable
        where TNative0 : unmanaged, IComponent
        where TNative1 : unmanaged, IComponent
        where TNative2 : unmanaged, IComponent {
        public NativeArray<int>      Components0Ids;
        public NativeArray<TNative0> Components0Values;
        public NativeArray<int>      Components1Ids;
        public NativeArray<TNative1> Components1Values;
        public NativeArray<int>      Components2Ids;
        public NativeArray<TNative2> Components2Values;

        public void Dispose() {
            this.Components0Ids.Dispose();
            this.Components0Values.Dispose();
            this.Components1Ids.Dispose();
            this.Components1Values.Dispose();
            this.Components2Ids.Dispose();
            this.Components2Values.Dispose();
        }
    }
}
#endif