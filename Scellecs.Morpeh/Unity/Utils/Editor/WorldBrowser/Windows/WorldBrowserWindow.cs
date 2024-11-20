#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.Utils.Editor {
    public class WorldBrowserWindow : EditorWindow {
        private const string VIEW_CONTAINER_STYLE_SHEET = "Packages/com.scellecs.morpeh/Unity/Utils/Editor/WorldBrowser/Styles/ViewContainer.uss";
        private const string HIERARCHY_SEARCH_STYLE_SHEET = "Packages/com.scellecs.morpeh/Unity/Utils/Editor/WorldBrowser/Styles/HierarchySearch.uss";
        private const string HIERARCHY_STYLE_SHEET = "Packages/com.scellecs.morpeh/Unity/Utils/Editor/WorldBrowser/Styles/Hierarchy.uss";
        private const string INSPECTOR_STYLE_SHEET = "Packages/com.scellecs.morpeh/Unity/Utils/Editor/WorldBrowser/Styles/Inspector.uss";

        private HierarchySearch hierarchySearch;
        private Hierarchy hierarchy;
        private Inspector inspector;

        private HierarchySearchView hierarchySearchView;
        private HierarchyView hierarchyView;
        private InspectorView inspectorView;

        private SplitterView splitterView;

        private StyleSheet viewContainerStyleSheet;
        private StyleSheet hierarchySearchStyleSheet;
        private StyleSheet hierarchyStyleSheet;
        private StyleSheet inspectorStyleSheet;

        [MenuItem("Tools/Morpeh/WorldBrowser")]
        private static void OpenWindow() {
            GetWindow<WorldBrowserWindow>();
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged += this.EditorApplicationOnPlayModeStateChanged;
            this.titleContent = new GUIContent("World Browser", Resources.Load<Texture>("MorpehIcons/64x64_W"), "Entities Browser");
        }

        private void OnDestroy() {
            EditorApplication.playModeStateChanged -= this.EditorApplicationOnPlayModeStateChanged;
            this.inspectorView?.Dispose();
        }

        private void CreateGUI() {
            if (!Application.isPlaying) {
                return;
            }

            this.rootVisualElement.Clear();

            this.hierarchySearch = new HierarchySearch();
            this.hierarchy = new Hierarchy(this.hierarchySearch);
            this.inspector = new Inspector(this.hierarchy);

            this.viewContainerStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(VIEW_CONTAINER_STYLE_SHEET);
            this.hierarchySearchStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(HIERARCHY_SEARCH_STYLE_SHEET);
            this.hierarchyStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(HIERARCHY_STYLE_SHEET);
            this.inspectorStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(INSPECTOR_STYLE_SHEET);

            this.splitterView = new SplitterView(250, 300);
            this.hierarchySearchView = new HierarchySearchView(this.hierarchySearch);
            this.hierarchyView = new HierarchyView(this.hierarchy);
            this.inspectorView = new InspectorView(this.inspector);

            this.rootVisualElement.styleSheets.Add(this.viewContainerStyleSheet);
            this.hierarchySearchView.styleSheets.Add(this.hierarchySearchStyleSheet);
            this.hierarchyView.styleSheets.Add(this.hierarchyStyleSheet);
            this.inspectorView.styleSheets.Add(this.inspectorStyleSheet);

            this.splitterView.hierarchySearchRoot.Add(this.hierarchySearchView);
            this.splitterView.hierarchyRoot.Add(this.hierarchyView);
            this.splitterView.inspectorRoot.Add(this.inspectorView);

            this.rootVisualElement.Add(this.splitterView.root);
        }

        private void Update() {
            if (!Application.isPlaying) {
                return;
            }

            this.hierarchySearch.Update();
            this.hierarchy.Update();
            this.inspector.Update();

            this.hierarchySearchView.Update();
            this.hierarchyView.Update();
            this.inspectorView.Update();
        }

        private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                this.inspectorView?.Dispose();
                this.rootVisualElement.Clear();
            }
        }
    }
}
#endif