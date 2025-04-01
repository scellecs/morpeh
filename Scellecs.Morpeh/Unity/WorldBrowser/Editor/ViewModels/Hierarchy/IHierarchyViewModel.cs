#if UNITY_EDITOR
using System.Collections;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal interface IHierarchyViewModel : IViewModel {
        public void Update();
        public bool IsSelectedWorldId(int id);
        public int GetSelectedEntityIndex();
        public int GetTotalEntitiesFound();
        public IList GetWorldsSource();
        public IList GetEntitiesSource();
        public void SetSelectedWorldId(int id, bool state);
        public void SetSelectedEntity(Entity entity);
    }
}
#endif
