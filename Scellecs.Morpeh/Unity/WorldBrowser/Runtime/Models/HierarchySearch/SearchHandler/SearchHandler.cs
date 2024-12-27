#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scellecs.Morpeh.WorldBrowser {
    internal class SearchHandler {
        private ComponentStorage componentsStorage;
        private SearchStringBuilder searchStringBuilder;

        private Tokenizer tokenizer;
        private List<Token> tokens;

        private TokenParser tokenParser;
        private TokenParser.ParseOutput parsedData;

        private string value;

        internal ReadOnlySpan<char> SpanValue => this.value.AsSpan();
        internal string StringValue => this.value;

        internal SearchHandler(ComponentStorage componentsStorage) {
            this.componentsStorage = componentsStorage;
            this.searchStringBuilder = new SearchStringBuilder();
            this.tokenParser = new TokenParser(this.componentsStorage);
            this.parsedData = default;
            this.value = string.Empty;
            this.tokens = new List<Token>();
            this.tokenizer = new Tokenizer();
            this.tokenizer.AddDefinition(new TokenDefinition(TokenType.Id, TokenType.Id.GetRegexPattern()));
            this.tokenizer.AddDefinition(new TokenDefinition(TokenType.Without, TokenType.Without.GetRegexPattern()));
            this.tokenizer.AddDefinition(new TokenDefinition(TokenType.With, TokenType.With.GetRegexPattern()));
        }

        internal void Refresh() {
            this.tokens = tokenizer.Tokenize(this.SpanValue, this.tokens);
            this.parsedData = tokenParser.ParseSequence(this.SpanValue, this.tokens);
        }

        internal void SetValue(string value) {
            this.value = value;
            this.Refresh();
        }

        internal void Fetch(SearchOutput output) {
            var with = output.GetOutput(QueryParam.With);
            var without = output.GetOutput(QueryParam.Without);

            with.UpdateUsed(this.parsedData.withComponents);
            without.UpdateUsed(this.parsedData.withoutComponents);

            var invalidWithName = ReadOnlySpan<char>.Empty;
            var invalidWithoutName = ReadOnlySpan<char>.Empty;

            if (this.HasInvalid(QueryParam.With, out var invalidWith)) {
                invalidWithName = this.tokens[invalidWith].GetValueWithoutPrefix(this.SpanValue);
            }

            if (this.HasInvalid(QueryParam.Without, out var invalidWithout)) {
                invalidWithoutName = this.tokens[invalidWithout].GetValueWithoutPrefix(this.SpanValue);
            }

            with.UpdateSuggestions(invalidWithName);
            without.UpdateSuggestions(invalidWithoutName);
            output.UpdateEntityIds(this.parsedData.ids);
        }

        internal bool TryGetFirstIndex(QueryParam param, int componentId, out int index) {
            var target = param == QueryParam.With ? this.parsedData.withComponents : this.parsedData.withoutComponents;
            var parsedToken = target.FirstOrDefault(token => token.value == componentId);
            index = parsedToken.sourceTokenIndex;
            return parsedToken.value == componentId;
        }

        internal bool HasInvalid(QueryParam queryParam, out int invalidIndex) {
            return this.TryGetFirstIndex(queryParam, SearchHelpers.INVALID_COMPONENT_ID, out invalidIndex);
        }

        internal void RebuildWithReplacement(int index, int componentId) {
            var name = this.componentsStorage.GetComponentNameById(componentId);
            var newValue = this.searchStringBuilder.BuildWithReplacement(this.tokens, this.SpanValue, index, name);
            this.SetValue(newValue);
        }

        internal void RebuildWithAddition(QueryParam param, int componentId) {
            var prefix = param.GetPrefix();
            var name = this.componentsStorage.GetComponentNameById(componentId);
            var newValue = this.searchStringBuilder.BuildWithAddition(this.tokens, this.SpanValue, name, prefix);
            this.SetValue(newValue);
        }

        internal void RebuildWithRemoval(QueryParam param, int componentId) {
            if (this.TryGetFirstIndex(param, componentId, out var tokenIndex)) {
                var newValue = this.searchStringBuilder.BuildWithRemoval(this.tokens, this.SpanValue, tokenIndex);
                this.SetValue(newValue);
            }
        }
    }
}
#endif