#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IHierarchyProcessor {
        public HierarchyModel GetModel();
        public void SetSelectedEntity(Entity entity);
        public void SetSelectedWorldId(int id, bool state);
    }
}
#endif
