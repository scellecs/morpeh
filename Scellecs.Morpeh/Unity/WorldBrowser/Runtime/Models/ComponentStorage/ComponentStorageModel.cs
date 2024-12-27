#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class ComponentStorageModel : BaseModel {
        internal IList<string> componentNames;
        internal IDictionary<int, int> typeIdToInternalId;
    }
}
#endif
