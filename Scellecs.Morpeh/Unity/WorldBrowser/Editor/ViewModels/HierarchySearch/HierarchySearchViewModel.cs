#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.Utils;
using System.Collections;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchySearchViewModel : BaseViewModel, IHierarchySearchViewModel {
        private readonly HierarchySearchModel model;
        private readonly ComponentStorageModel storage;
        private readonly IHierarchySearchProcessor processor;

        private readonly VirtualList<int> withSource;
        private readonly VirtualList<int> withoutSource;

        private long modelVersion;

        internal HierarchySearchViewModel(IHierarchySearchProcessor processor, IComponentStorageProcessor storageProcessor) {
            this.model = processor.GetModel();
            this.processor = processor;
            this.storage = storageProcessor.GetModel();
            this.withSource = new VirtualList<int>(this.model.withSource);
            this.withoutSource = new VirtualList<int>(this.model.withoutSource);
            this.version = 0u;
            this.modelVersion = -1u;
            this.IncrementVersion();
        }

        public void Update() {
            if (this.model.version != this.modelVersion) {
                this.modelVersion = this.model.version;
                this.withSource.SetList(this.model.withSource);
                this.withoutSource.SetList(this.model.withoutSource);
                this.IncrementVersion();
            }
        }

        public string GetSearchString() {
            return this.model.searchString;
        }

        public bool GetComponentIncluded(int id, QueryParam param) { 
            var target = param == QueryParam.With ? this.model.includedIds : this.model.excludedIds;
            return target.Contains(id);
        }

        public string GetComponentNameById(int id) {
            return this.storage.componentNames[id];
        }

        public IList GetItemsSource(QueryParam param) {
            return param == QueryParam.With ? this.withSource : this.withoutSource;
        }

        public void SetSearchString(string search) {
            this.processor.SetSearchString(search);
        }

        public void SetComponentIncluded(int id, bool included, QueryParam param) {
            this.processor.SetComponentIncluded(id, included, param);
        }
    }
}
#endif
