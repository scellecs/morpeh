#if UNITY_EDITOR
using System.Collections;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class InspectorListViewController : ListViewController {
        private readonly IInspectorViewModel model;

        public override IList itemsSource => this.model.GetEntityComponentsSource();

        internal InspectorListViewController(IInspectorViewModel model) : base() {
            this.model = model;
        }
    }
}
#endif