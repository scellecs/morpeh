#if UNITY_EDITOR
using System.Collections;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal interface IHierarchySearchViewModel : IViewModel {
        public void Update();
        public string GetSearchString();
        public void SetSearchString(string value);
        public bool GetComponentIncluded(int id, QueryParam queryParam);
        public void SetComponentIncluded(int id, bool included, QueryParam param);
        public string GetComponentNameById(int id);
        public IList GetItemsSource(QueryParam param);
    }
}
#endif
