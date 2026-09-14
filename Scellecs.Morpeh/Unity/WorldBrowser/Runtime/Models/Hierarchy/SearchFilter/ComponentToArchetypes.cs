#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Collections.Generic;
namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal struct ComponentToArchetypes {
        internal HashSet<long> value;
    }
}
#endif
