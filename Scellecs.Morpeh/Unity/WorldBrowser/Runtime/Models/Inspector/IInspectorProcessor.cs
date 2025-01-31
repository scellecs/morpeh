#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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