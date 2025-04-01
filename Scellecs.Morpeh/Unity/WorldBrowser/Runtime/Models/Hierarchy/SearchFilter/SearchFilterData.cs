#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal sealed class SearchFilterData {
        internal readonly List<int> inc;
        internal readonly List<int> exc;
        internal readonly List<int> ids;
        internal bool isValid;

        internal SearchFilterData() {
            this.inc = new List<int>();
            this.exc = new List<int>();
            this.ids = new List<int>();
            this.isValid = true;
        }

        internal void Clear() { 
            this.inc.Clear();
            this.exc.Clear();
            this.ids.Clear();
            this.isValid = true;
        }
    }
}
#endif