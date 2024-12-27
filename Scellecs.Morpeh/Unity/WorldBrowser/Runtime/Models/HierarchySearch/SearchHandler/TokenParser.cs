#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class TokenParser {
        internal readonly struct ParsedToken {
            internal readonly int sourceTokenIndex;
            internal readonly int value;

            internal ParsedToken(int sourceTokenIndex, int value) {
                this.sourceTokenIndex = sourceTokenIndex;
                this.value = value;
            }
        }

        internal struct ParseOutput {
            internal List<ParsedToken> withComponents;
            internal List<ParsedToken> withoutComponents;
            internal List<ParsedToken> ids;
        }

        private readonly ComponentStorage componentsStorage;
        private readonly List<ParsedToken> withComponentsBuffer;
        private readonly List<ParsedToken> withoutComponentsBuffer;
        private readonly List<ParsedToken> idsBuffer;
        private readonly List<int> indicesBuffer;

        internal TokenParser(ComponentStorage componentsStorage) {
            this.componentsStorage = componentsStorage;
            this.withComponentsBuffer = new List<ParsedToken>();
            this.withoutComponentsBuffer = new List<ParsedToken>();
            this.idsBuffer = new List<ParsedToken>();
            this.indicesBuffer = new List<int>();
        }

        internal ParseOutput ParseSequence(ReadOnlySpan<char> sourceText, List<Token> tokens) {
            this.withComponentsBuffer.Clear();
            this.withoutComponentsBuffer.Clear();
            this.idsBuffer.Clear();

            var output = new ParseOutput() {
                withComponents = this.withComponentsBuffer,
                withoutComponents = this.withoutComponentsBuffer,
                ids = this.idsBuffer,
            };

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                switch (token.tokenType) {
                    case TokenType.Id:
                        ParseId(token.GetValueWithoutPrefix(sourceText), i, output.ids);
                        break;

                    case TokenType.Without:
                        ParseQueryParam(token.GetValueWithoutPrefix(sourceText), i, output.withoutComponents);
                        break;

                    case TokenType.With:
                        ParseQueryParam(token.GetValueWithoutPrefix(sourceText), i, output.withComponents);
                        break;
                }
            }

            return output;
        }

        private void ParseId(ReadOnlySpan<char> text, int tokenIndex, List<ParsedToken> target) {
            if (int.TryParse(text, out int idValue)) {
                target.Add(new ParsedToken(tokenIndex, idValue));
            }
        }

        private void ParseQueryParam(ReadOnlySpan<char> text, int tokenIndex, List<ParsedToken> target) { 
            var indices = this.componentsStorage.GetComponentIdsMatchesWithPrefix(text, this.indicesBuffer);
            int componentId = SearchHelpers.INVALID_COMPONENT_ID;

            foreach (var idx in indices) {
                if (this.componentsStorage.GetComponentNameById(idx).AsSpan().SequenceEqual(text)) {
                    componentId = idx;
                    break;
                }
            }

            target.Add(new ParsedToken(tokenIndex, componentId));
        }
    }
}
#endif
