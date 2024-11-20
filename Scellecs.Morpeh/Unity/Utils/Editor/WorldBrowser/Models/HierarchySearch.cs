#if UNITY_EDITOR
using System.Collections;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class HierarchySearch {
        private readonly ComponentsStorage componentsStorage;
        private readonly SearchHandler searchHandler;
        private readonly SearchOutput searchOutput;

        private long version;

        internal HierarchySearch() {
            this.componentsStorage = new ComponentsStorage();
            this.searchHandler = new SearchHandler(this.componentsStorage);
            this.searchOutput = new SearchOutput(this.componentsStorage);
            this.version = 0u;
        }

        internal void Update() {
            if (this.componentsStorage.ValidateUpdateCache()) {
                this.searchHandler.Refresh();
                this.Fetch();
            }
        }

        internal void FillSearchFilterData(SearchFilterData filterData) {
            this.searchOutput.FillFilterData(filterData);
        }

        internal string GetComponentName(int id) {
            return this.componentsStorage.componentNames[id];
        }

        internal string GetComponentNameByTypeId(int typeId) {
            return this.componentsStorage.componentNames[this.componentsStorage.typeIdToInternalId[typeId]];
        }

        internal string GetSearchString() {
            return this.searchHandler.StringValue;
        }

        internal bool GetComponentIncluded(int id, QueryParam param) {
            return this.searchOutput.Has(param, id);
        }

        internal IList GetItemsSource(QueryParam param) {
            return this.searchOutput.GetSource(param);
        }

        internal long GetVersion() {
            return this.version;
        }

        internal void SetSearchString(string input) {
            this.searchHandler.SetValue(input);
            this.Fetch();
        }

        internal void SetComponentIncluded(int id, bool included, QueryParam param) {
            if (this.searchHandler.HasInvalid(param, out var index)) {
                this.searchHandler.RebuildWithReplacement(index, id);
                this.Fetch();
                return;
            }

            if (included) {
                this.searchHandler.RebuildWithAddition(param, id);
            }
            else {
                this.searchHandler.RebuildWithRemoval(param, id);
            }

            this.Fetch();
        }

        private void Fetch() {
            this.searchHandler.Fetch(this.searchOutput);
            unchecked {
                this.version++;
            }
        }
    }
}
#endif