#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class HierarchyModel : BaseModel {
        internal IList<int> worldIds;
        internal ISet<int> selectedWorldIds;
        internal IList<Entity> entities;
    }
}
#endif
