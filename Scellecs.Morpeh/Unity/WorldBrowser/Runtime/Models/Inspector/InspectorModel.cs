#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class InspectorModel : BaseModel {
        internal IList<ComponentDataBoxed> components;
        internal Entity selectedEntity;
    }
}
#endif