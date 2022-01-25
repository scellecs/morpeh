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
}