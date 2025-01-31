#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Scellecs.Morpeh.WorldBrowser {
    internal enum TokenType {
        Id = 0,
        Without = 1,
        With = 2,
    }

    internal ref struct TokenMatch {
        internal bool isMatch;
        internal int startIndex;
        internal int length;
        internal TokenType tokenType;
        internal ReadOnlySpan<char> remainingText;
    }

    internal readonly struct TokenDefinition {
        private readonly Regex regex;
        private readonly TokenType returnsToken;

        internal TokenDefinition(TokenType returnsToken, string regexPattern) {
            this.regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            this.returnsToken = returnsToken;
        }

        internal TokenMatch Match(ReadOnlySpan<char> inputText) {
            var match = this.regex.Match(inputText.ToString()); //TODO
            if (match.Success) {
                return new TokenMatch {
                    isMatch = true,
                    startIndex = 0,
                    length = match.Length,
                    tokenType = this.returnsToken,
                    remainingText = inputText[match.Length..]
                };
            }

            return new TokenMatch { isMatch = false };
        }
    }

    internal readonly struct Token {
        internal readonly int startIndex;
        internal readonly int length;
        internal readonly TokenType tokenType;

        internal Token(TokenType tokenType, int startIndex, int length)  {
            this.tokenType = tokenType;
            this.startIndex = startIndex;
            this.length = length;
        }

        internal ReadOnlySpan<char> GetValue(ReadOnlySpan<char> sourceText) {
            return sourceText.Slice(this.startIndex, this.length);
        }
    }

    internal sealed class Tokenizer {
        private readonly List<TokenDefinition> tokenDefinitions;

        internal Tokenizer() {
            this.tokenDefinitions = new List<TokenDefinition>();
        }

        internal void AddDefinition(TokenDefinition definition) {
            this.tokenDefinitions.Add(definition);
        }

        internal List<Token> Tokenize(ReadOnlySpan<char> inputText, List<Token> buffer = null) {
            buffer ??= new List<Token>();
            buffer.Clear();
            var remainingText = inputText;
            var currentIndex = inputText.Length - remainingText.Length;

            while (!remainingText.IsEmpty) {
                var match = FindMatch(remainingText);
                if (match.isMatch) {
                    buffer.Add(new Token(match.tokenType, currentIndex, match.length));
                    currentIndex += match.length;
                    remainingText = match.remainingText;
                }
                else {
                    remainingText = remainingText[1..];
                    currentIndex++;
                }
            }

            return buffer;
        }

        private TokenMatch FindMatch(ReadOnlySpan<char> text) {
            foreach (var tokenDefinition in this.tokenDefinitions) {
                var match = tokenDefinition.Match(text);
                if (match.isMatch) {
                    return match;
                }
            }

            return new TokenMatch { isMatch = false };
        }
    }
}
#endif