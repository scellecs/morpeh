#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IModelsStorage : IDisposable { 
        public IComponentStorageProcessor ComponentStorageProcessor { get; }
        public IHierarchySearchProcessor HierarchySearchProcessor { get; }
        public IHierarchyProcessor HierarchyProcessor { get; }
        public IInspectorProcessor InspectorProcessor { get; }
        public bool Initialize();
        public void Update();
    }
}
#endif