#if UNITY_EDITOR
using System.Collections;
using UnityEngine.UIElements;
using HierarchyModel = Scellecs.Morpeh.Utils.Editor.Hierarchy;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class HierarchyListViewController : ListViewController {
        private readonly HierarchyModel model;

        public override IList itemsSource => this.model.GetEntitiesSource();

        internal HierarchyListViewController(HierarchyModel model) : base() {
            this.model = model;
        }
    }
}
#endif

