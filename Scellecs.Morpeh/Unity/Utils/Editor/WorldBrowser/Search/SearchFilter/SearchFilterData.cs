#if UNITY_EDITOR
using System.Collections.Generic;
namespace Scellecs.Morpeh.Utils.Editor {
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