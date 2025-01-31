#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;
namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal struct ComponentToArchetypes {
        internal HashSet<long> value;
    }
}
#endif
