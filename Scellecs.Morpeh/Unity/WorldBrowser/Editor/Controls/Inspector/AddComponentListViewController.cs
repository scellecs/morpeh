#if UNITY_EDITOR
using System.Collections;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class AddComponentListViewController : ListViewController {
        private readonly IInspectorViewModel model;

        public override IList itemsSource => this.model.GetAddComponentSuggestionsSource();

        internal AddComponentListViewController(IInspectorViewModel model) : base() {
            this.model = model;
        }
    }
}
#endif