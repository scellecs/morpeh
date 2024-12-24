#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.ComponentViewer;
using System.Collections;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal interface IInspectorViewModel : IViewModel {
        public void Update();
        public void SetExpanded(int typeId, bool value);
        public void SetExpandedAll(bool value);
        public bool IsExpanded(int typeId);
        public bool IsNotExpandedAll();
        public IList GetItemsSource();
        public ComponentData GetComponentData(int typeId);
    }
}
#endif
