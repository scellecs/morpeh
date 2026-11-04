#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class HierarchyModel : BaseModel {
        internal IList<int> worldIds;
        internal ISet<int> selectedWorldIds;
        internal IList<Entity> entities;
    }
}
#endif
