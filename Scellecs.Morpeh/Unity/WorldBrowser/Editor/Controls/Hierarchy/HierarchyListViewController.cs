#if UNITY_EDITOR
using System.Collections;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchyListViewController : ListViewController {
        private readonly IHierarchyViewModel model;

        public override IList itemsSource => this.model.GetEntitiesSource();

        internal HierarchyListViewController(IHierarchyViewModel model) : base() {
            this.model = model;
        }
    }
}
#endif

