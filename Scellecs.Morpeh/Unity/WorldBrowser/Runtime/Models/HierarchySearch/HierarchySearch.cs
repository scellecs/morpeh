#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Filter;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class HierarchySearch : IHierarchySearchProcessor {
        private readonly HierarchySearchModel model;
        private readonly ComponentStorage componentsStorage;
        private readonly SearchHandler searchHandler;
        private readonly SearchOutput searchOutput;

        private long storageVersion;

        internal HierarchySearch(ComponentStorage componentsStorage) {
            this.model = new HierarchySearchModel();
            this.componentsStorage = componentsStorage;
            this.searchHandler = new SearchHandler(this.componentsStorage);
            this.searchOutput = new SearchOutput(this.componentsStorage);
            this.storageVersion = -1u;
            this.UpdateModel();
        }

        internal void Update() {
            if (this.storageVersion != this.componentsStorage.GetVersion()) {
                this.storageVersion = this.componentsStorage.GetVersion();
                this.searchHandler.Refresh();
                this.FetchSearch();
                this.UpdateModel();
            }
        }

        internal void FillSearchFilterData(SearchFilterData filterData) {
            this.searchOutput.FillFilterData(filterData);
        }

        public HierarchySearchModel GetModel() { 
            return this.model; 
        }

        public void SetSearchString(string input) {
            this.searchHandler.SetValue(input);
            this.FetchSearch();
            this.UpdateModel();
        }

        public void SetComponentIncluded(int id, bool included, QueryParam param) {
            if (this.searchHandler.HasInvalid(param, out var index)) {
                this.searchHandler.RebuildWithReplacement(index, id);
                this.FetchSearch();
                this.UpdateModel();
                return;
            }

            if (included) {
                this.searchHandler.RebuildWithAddition(param, id);
            }
            else {
                this.searchHandler.RebuildWithRemoval(param, id);
            }

            this.FetchSearch();
            this.UpdateModel();
        }

        public long GetVersion() {
            return this.model.version;
        }

        private void FetchSearch() {
            this.searchHandler.Fetch(this.searchOutput);
        }

        private void UpdateModel() {
            this.model.searchString = this.searchHandler.StringValue;
            this.model.withSource = this.searchOutput.GetSource(QueryParam.With);
            this.model.withoutSource = this.searchOutput.GetSource(QueryParam.Without);
            this.model.includedIds = this.searchOutput.GetOutput(QueryParam.With).GetUsedIds();
            this.model.excludedIds = this.searchOutput.GetOutput(QueryParam.Without).GetUsedIds();
            this.model.IncrementVersion();
        }
    }
}
#endif