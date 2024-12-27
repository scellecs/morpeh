#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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