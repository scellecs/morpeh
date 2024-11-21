#if UNITY_EDITOR
using System.Collections;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class InspectorListViewController : ListViewController {
        private readonly Inspector model;

        public override IList itemsSource => this.model.GetItemsSource();

        internal InspectorListViewController(Inspector model) : base() {
            this.model = model;
        }
    }
}
#endif