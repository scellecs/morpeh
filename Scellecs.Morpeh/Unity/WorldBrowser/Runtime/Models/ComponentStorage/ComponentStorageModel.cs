#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class ComponentStorageModel : BaseModel {
        internal IList<string> componentNames;
        internal IDictionary<int, int> typeIdToInternalId;
    }
}
#endif
