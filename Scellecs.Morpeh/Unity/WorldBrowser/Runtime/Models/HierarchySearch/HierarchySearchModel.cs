#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class HierarchySearchModel : BaseModel {
        internal string searchString;
        internal IList<int> withSource;
        internal IList<int> withoutSource;
        internal ISet<int> includedIds;
        internal ISet<int> excludedIds;
    }
}
#endif