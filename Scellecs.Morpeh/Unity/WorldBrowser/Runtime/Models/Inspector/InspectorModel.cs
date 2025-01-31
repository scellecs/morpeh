#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class InspectorModel : BaseModel {
        internal IList<ComponentDataBoxed> components;
        internal IList<int> addComponentSuggestions;
        internal Entity selectedEntity;
    }
}
#endif