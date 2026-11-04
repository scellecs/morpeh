namespace Scellecs.Morpeh {
    using System;
    
    public interface IInitializer : IDisposable {
        World World { get; }

        void OnAwake();
    }
}