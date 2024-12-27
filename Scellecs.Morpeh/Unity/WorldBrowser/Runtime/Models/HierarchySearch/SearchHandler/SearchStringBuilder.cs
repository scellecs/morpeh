#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Text;
namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class SearchStringBuilder {
        private readonly StringBuilder stringBuilder;

        internal SearchStringBuilder() {
            this.stringBuilder = new StringBuilder();
        }

        internal string BuildWithReplacement(List<Token> tokens, ReadOnlySpan<char> sourceText, int replacementIndex, string replacementValue) {
            this.stringBuilder.Clear();
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                if (i == replacementIndex) {
                    var prefix = token.tokenType.GetPrefix();
                    this.stringBuilder.Append(prefix);
                    this.stringBuilder.Append(replacementValue);
                }
                else {
                    this.stringBuilder.Append(token.GetValue(sourceText));
                }

                this.stringBuilder.Append(" ");
            }

            this.stringBuilder.Length = TrimEnd(this.stringBuilder);
            return this.stringBuilder.ToString();
        }

        internal string BuildWithRemoval(List<Token> tokens, ReadOnlySpan<char> sourceText, int removeIndex) {
            this.stringBuilder.Clear();
            for (int i = 0; i < tokens.Count; i++) {
                if (i == removeIndex) {
                    continue;
                }

                var token = tokens[i];
                this.stringBuilder.Append(token.GetValue(sourceText));
                this.stringBuilder.Append(" ");
            }

            this.stringBuilder.Length = TrimEnd(this.stringBuilder);
            return this.stringBuilder.ToString();
        }

        internal string BuildWithAddition(List<Token> tokens, ReadOnlySpan<char> sourceText, string additionValue, string prefix) {
            this.stringBuilder.Clear();
            this.stringBuilder.Append(prefix);
            this.stringBuilder.Append(additionValue); 
            this.stringBuilder.Append(" ");

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                this.stringBuilder.Append(token.GetValue(sourceText));
                this.stringBuilder.Append(" ");
            }

            this.stringBuilder.Length = TrimEnd(this.stringBuilder);
            return this.stringBuilder.ToString();
        }

        private static int TrimEnd(StringBuilder stringBuilder) {
            return stringBuilder.Length > 0 ? stringBuilder.Length - 1 : stringBuilder.Length;
        }
    }
}
#endif
