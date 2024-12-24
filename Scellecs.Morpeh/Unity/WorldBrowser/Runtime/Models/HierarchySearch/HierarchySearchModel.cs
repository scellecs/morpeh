#if UNITY_EDITOR || DEVELOPMENT_BUILD
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