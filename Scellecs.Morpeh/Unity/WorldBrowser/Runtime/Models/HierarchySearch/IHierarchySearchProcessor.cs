#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IHierarchySearchProcessor {
        public HierarchySearchModel GetModel();
        public void SetSearchString(string value);
        public void SetComponentIncluded(int id, bool included, QueryParam param);
    }
}
#endif