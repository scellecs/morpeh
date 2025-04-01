#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class HierarchySearchView : VisualElement {
        private const string HIERARCHY_SEARCH = "hierarchy-search";
        private const string SEARCH_INFO_CONTAINER = "hierarchy-search-info-container";
        private const string WITH_INFO_LABEL = "hierarchy-search-with-label";
        private const string WITHOUT_INFO_LABEL = "hierarchy-search-without-label";
        private const string SEARCH_LISTS_CONTAINER = "hierarchy-search-components-list-container";
        private const string SEARCH_LIST = "hierarchy-search-components-list";
        private const string INPUT_FIELD_CONTAINER = "hierarchy-search-regex-container";
        private const string INPUT_FIELD = "hierarchy-search-regex-text";

        private readonly VisualElement inputFieldContainer;
        private readonly TextField inputField;

        private readonly VisualElement searchInfoContainer;
        private readonly VisualElement withLabel;
        private readonly VisualElement withoutLabel;
        private readonly SearchInfoTooltip searchInfoTooltip;

        private readonly VisualElement listsContainer;
        private readonly HierarchySearchListView withListView;
        private readonly HierarchySearchListView withoutListView;

        private readonly IHierarchySearchViewModel model;
        private long modelVersion;

        internal HierarchySearchView(IHierarchySearchViewModel model) {
            this.model = model;
            this.modelVersion = -1u;

            this.AddToClassList(HIERARCHY_SEARCH);

            this.inputFieldContainer = new VisualElement();
            this.inputFieldContainer.AddToClassList(INPUT_FIELD_CONTAINER);
            this.inputField = new TextField();
            this.inputField.AddToClassList(INPUT_FIELD);
            this.inputField.RegisterValueChangedCallback((evt) => this.model.SetSearchString(evt.newValue));
            this.inputFieldContainer.Add(this.inputField);

            this.listsContainer = new VisualElement();
            this.listsContainer.AddToClassList(SEARCH_LISTS_CONTAINER);

            this.searchInfoContainer = new VisualElement();
            this.searchInfoContainer.AddToClassList(SEARCH_INFO_CONTAINER);

            this.withLabel = new Label("With");
            this.withoutLabel = new Label("Without");
            this.withLabel.AddToClassList(WITH_INFO_LABEL);
            this.withoutLabel.AddToClassList(WITHOUT_INFO_LABEL);

            this.searchInfoTooltip = new SearchInfoTooltip();
            this.searchInfoTooltip.AddTooltipHandler("Write id: to search for entities by ID.\nSearching by ID overrides any query request.");

            this.searchInfoContainer.Add(this.withLabel);
            this.searchInfoContainer.Add(this.withoutLabel);
            this.searchInfoContainer.Add(this.searchInfoTooltip);

            this.withListView = new HierarchySearchListView(this.model, QueryParam.With);
            this.withoutListView = new HierarchySearchListView(this.model, QueryParam.Without);
            this.withListView.AddToClassList(SEARCH_LIST);
            this.withoutListView.AddToClassList(SEARCH_LIST);

            this.listsContainer.Add(this.withListView);
            this.listsContainer.Add(this.withoutListView);

            this.Add(this.inputFieldContainer);
            this.Add(this.searchInfoContainer);
            this.Add(this.listsContainer);
        }

        internal void Update() {
            if (this.modelVersion != this.model.GetVersion()) {
                this.SyncWithModel();
            }
        }

        internal void SyncWithModel() {
            this.inputField.SetValueWithoutNotify(this.model.GetSearchString());
            this.withListView.UpdateItemsSource();
            this.withoutListView.UpdateItemsSource();
            this.modelVersion = this.model.GetVersion();
        }
    }
}
#endif