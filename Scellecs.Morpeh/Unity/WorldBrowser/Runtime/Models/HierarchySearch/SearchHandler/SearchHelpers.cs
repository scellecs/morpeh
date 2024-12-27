#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    internal static class SearchHelpers {
        internal const string ID_PREFIX = "id:";
        internal const string WITHOUT_PREFIX = "!";
        internal const string WITH_PREFIX = "";

        internal const string ID_PATTERN = @"^id:[^\s]+";
        internal const string WITHOUT_PATTERN = @"^\![^\s]*";
        internal const string WITH_PATTERN = @"^[^\s]+";

        internal const int INVALID_COMPONENT_ID = -1;

        internal static string GetPrefix(this QueryParam param) {
            return param == QueryParam.With ? WITH_PREFIX : WITHOUT_PREFIX;
        }

        internal static string GetPrefix(this TokenType tokenType) {
            return tokenType switch {
                TokenType.Id => ID_PREFIX,
                TokenType.Without => WITHOUT_PREFIX,
                TokenType.With => WITH_PREFIX,
                _ => string.Empty,
            };
        }

        internal static string GetRegexPattern(this TokenType tokenType) {
            return tokenType switch {
                TokenType.Id => ID_PATTERN,
                TokenType.Without => WITHOUT_PATTERN,
                TokenType.With => WITH_PATTERN,
                _ => string.Empty,
            };
        }

        internal static ReadOnlySpan<char> GetValueWithoutPrefix(this Token token, ReadOnlySpan<char> sourceText) {
            var value = token.GetValue(sourceText);
            return value[token.tokenType.GetPrefix().Length..];
        }
    }
}
#endif