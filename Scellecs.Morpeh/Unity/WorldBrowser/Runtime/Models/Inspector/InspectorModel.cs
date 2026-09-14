#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class InspectorModel : BaseModel {
        internal IList<ComponentDataBoxed> components;
        internal IList<int> addComponentSuggestions;
        internal Entity selectedEntity;
    }
}
#endif