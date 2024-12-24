#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IInspectorProcessor {
        public InspectorModel GetModel();
        public void SetComponentData(int typeId, object data);
    }
}
#endif