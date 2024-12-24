#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class HierarchyModel : BaseModel {
        internal IList<int> worldIds;
        internal ISet<int> selectedWorldIds;
        internal IList<Entity> entities;
    }
}
#endif
