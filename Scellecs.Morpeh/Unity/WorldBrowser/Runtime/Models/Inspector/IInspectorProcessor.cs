#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IInspectorProcessor {
        public InspectorModel GetModel();
        public void AddComponentData(int id);
        public void RemoveComponentData(int typeId);
        public void SetComponentData(ComponentDataBoxed componentData);
        public void SetAddComponentSearchString(string value);
    }
}
#endif