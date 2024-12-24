#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser {
    internal interface IHierarchyProcessor {
        public HierarchyModel GetModel();
        public void SetSelectedEntity(Entity entity);
        public void SetSelectedWorldId(int id, bool state);
    }
}
#endif
