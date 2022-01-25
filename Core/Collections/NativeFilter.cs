namespace morpeh.Core.Collections {
    using System;
    using Morpeh;
    using Unity.Collections;

    public struct NativeFilter<T0> : IDisposable
        where T0 : unmanaged, IComponent {
        public NativeArray<int> Components0Ids;
        public NativeArray<T0>  Components0Values;
        
        public void Dispose() {
            this.Components0Ids.Dispose();
            this.Components0Values.Dispose();
        }
    }
    
    public struct NativeFilter<T0, T1> : IDisposable
        where T0 : unmanaged, IComponent
        where T1 : unmanaged, IComponent {
        public NativeArray<int> Components0Ids;
        public NativeArray<T0>  Components0Values;
        public NativeArray<int> Components1Ids;
        public NativeArray<T1>  Components1Values;

        public void Dispose() {
            this.Components0Ids.Dispose();
            this.Components0Values.Dispose();
            this.Components1Ids.Dispose();
            this.Components1Values.Dispose();
        }
    }
}