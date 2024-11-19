#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class SearchOutput {
        private readonly SearchComponentData outputWith;
        private readonly SearchComponentData outputWithout;
        private readonly ComponentsStorage componentsStorage;
        private readonly HashSet<int> ids;

        internal SearchOutput(ComponentsStorage componentsStorage) {
            this.componentsStorage = componentsStorage;
            this.outputWith = new SearchComponentData(this.componentsStorage);
            this.outputWithout = new SearchComponentData(this.componentsStorage);
            this.ids = new HashSet<int>();
        }

        internal void FillFilterData(SearchFilterData searchData) { 
            searchData.Clear();

            if (this.ids.Count > 0) {
                searchData.ids.AddRange(this.ids);
                searchData.isValid = true;
                return;
            }

            var with = this.outputWith;
            var without = this.outputWithout;

            if (!with.isValid || !without.isValid) {
                searchData.isValid = false;
                return;
            }

            foreach (var componentId in with.usedIds) {
                var typeId = this.componentsStorage.componentTypeIds[componentId];
                searchData.inc.Add(typeId);
            }

            foreach (var componentId in without.usedIds) {
                var typeId = this.componentsStorage.componentTypeIds[componentId];
                searchData.exc.Add(typeId);
            }
        }

        internal SearchComponentData GetOutput(QueryParam queryParam) {
            return queryParam == QueryParam.With ? this.outputWith : this.outputWithout;
        }

        internal IList GetSource(QueryParam queryParam) {
            return GetOutput(queryParam).GetSource();
        }

        internal bool Has(QueryParam queryParam, int componentId) { 
            return GetOutput(queryParam).Has(componentId);
        }

        internal void UpdateEntityIds(List<TokenParser.ParsedToken> parsedTokens) {
            this.ids.Clear();
            foreach (var token in parsedTokens) {
                this.ids.Add(token.value);
            }
        }
    }

    internal sealed class SearchComponentData {
        internal readonly List<int> indicesBuffer;
        internal readonly List<int> suggestions;
        internal readonly HashSet<int> usedIds;
        internal readonly ComponentsStorage componentsStorage;

        internal bool isValid;

        internal SearchComponentData(ComponentsStorage componentsStorage) {
            this.indicesBuffer = new List<int>();
            this.suggestions = new List<int>();
            this.usedIds = new HashSet<int>();
            this.componentsStorage = componentsStorage;
            this.isValid = true;
        }

        internal IList GetSource() {
            return isValid ? this.componentsStorage.componentIds : this.suggestions;
        }

        internal bool Has(int componentId) { 
            return usedIds.Contains(componentId);
        }

        internal void UpdateUsed(List<TokenParser.ParsedToken> parsedTokens) { 
            this.usedIds.Clear();
            foreach (var token in parsedTokens) {
                var id = token.value;
                if (id != -1) {
                    usedIds.Add(id);
                }
            }
        }

        internal void UpdateSuggestions(ReadOnlySpan<char> invalidName) {
            this.isValid = invalidName.IsEmpty;
            if (!this.isValid) {
                var suggestionsIndices = this.componentsStorage.componentsSearchTree.GetWordIndicesWithPrefix(invalidName, this.indicesBuffer);
                this.suggestions.Clear();
                this.suggestions.AddRange(suggestionsIndices.Where(idx => !this.usedIds.Contains(idx)));
            }
        }
    }
}
#endif