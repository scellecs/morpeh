#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.Utils;
using Scellecs.Morpeh.WorldBrowser.Remote;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    public class WorldBrowserWindow : EditorWindow {
        private const string VIEW_CONTAINER_STYLE_SHEET = "b573552224b286f448bfede0ca7ea76e";
        private const string HIERARCHY_SEARCH_STYLE_SHEET = "aa36bbbd296c0d749b070ca4ed146576";
        private const string HIERARCHY_STYLE_SHEET = "c411609523402cd49b3745cc501de47e";
        private const string INSPECTOR_STYLE_SHEET = "7a3a1c07f894eed4eb993bfae61a441a";

        private IModelsStorage modelsStorage;
        private IHierarchySearchViewModel hierarchySearchViewModel;
        private IHierarchyViewModel hierarchyViewModel;
        private IInspectorViewModel inspectorViewModel;

        private HierarchySearchView hierarchySearchView;
        private HierarchyView hierarchyView;
        private InspectorView inspectorView;
        private SplitterView splitterView;

        private StyleSheet viewContainerStyleSheet;
        private StyleSheet hierarchySearchStyleSheet;
        private StyleSheet hierarchyStyleSheet;
        private StyleSheet inspectorStyleSheet;

        private bool IsRemoteMode => !Application.isPlaying;
        private bool initialized;

        [MenuItem("Tools/Morpeh/WorldBrowser")]
        private static void OpenWindow() {
            GetWindow<WorldBrowserWindow>();
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged += this.EditorApplicationOnPlayModeStateChanged;
            this.titleContent = new GUIContent("World Browser", Resources.Load<Texture>("MorpehIcons/64x64_W"), "Entities Browser");
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= this.EditorApplicationOnPlayModeStateChanged;
            this.inspectorView?.Dispose();
            this.modelsStorage?.Dispose();
        }

        private void CreateGUI() {
            if (this.initialized) {
                return;
            }

            this.rootVisualElement.Clear();
            this.modelsStorage = IsRemoteMode ? new RemoteModelsStorage() : new ModelsStorage();

            if (!this.modelsStorage.Initialize()) {
                return;
            }

            this.hierarchySearchViewModel = new HierarchySearchViewModel(this.modelsStorage.HierarchySearchProcessor, this.modelsStorage.ComponentStorageProcessor);
            this.hierarchyViewModel = new HierarchyViewModel(this.modelsStorage.HierarchyProcessor);
            this.inspectorViewModel = new InspectorViewModel(this.modelsStorage.InspectorProcessor, this.modelsStorage.ComponentStorageProcessor);

            this.viewContainerStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(VIEW_CONTAINER_STYLE_SHEET);
            this.hierarchySearchStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(HIERARCHY_SEARCH_STYLE_SHEET);
            this.hierarchyStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(HIERARCHY_STYLE_SHEET);
            this.inspectorStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(INSPECTOR_STYLE_SHEET);

            this.splitterView = new SplitterView(250, 300);
            this.hierarchySearchView = new HierarchySearchView(this.hierarchySearchViewModel);
            this.hierarchyView = new HierarchyView(this.hierarchyViewModel);
            this.inspectorView = new InspectorView(this.inspectorViewModel);

            this.rootVisualElement.styleSheets.Add(this.viewContainerStyleSheet);
            this.hierarchySearchView.styleSheets.Add(this.hierarchySearchStyleSheet);
            this.hierarchyView.styleSheets.Add(this.hierarchyStyleSheet);
            this.inspectorView.styleSheets.Add(this.inspectorStyleSheet);

            this.splitterView.hierarchySearchRoot.Add(this.hierarchySearchView);
            this.splitterView.hierarchyRoot.Add(this.hierarchyView);
            this.splitterView.inspectorRoot.Add(this.inspectorView);

            this.rootVisualElement.Add(this.splitterView.root);
            this.initialized = true;
        }

        private void Update() {
            if (!this.initialized) {
                return;
            }

            this.modelsStorage.Update();

            this.hierarchySearchViewModel.Update();
            this.hierarchyViewModel.Update();
            this.inspectorViewModel.Update();

            this.hierarchySearchView.Update();
            this.hierarchyView.Update();
            this.inspectorView.Update();
        }

        private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                this.inspectorView?.Dispose();
                this.rootVisualElement.Clear();
                this.initialized = false;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode) {
                this.CreateGUI();
            }
        }
    }
}
#endif