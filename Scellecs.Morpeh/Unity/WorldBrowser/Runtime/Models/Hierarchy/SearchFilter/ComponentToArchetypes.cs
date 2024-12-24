#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal struct ComponentToArchetypes {
        internal HashSet<long> value;
    }
}
#endif
