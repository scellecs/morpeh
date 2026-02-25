#if UNITY_EDITOR
using Scellecs.Morpeh.WorldBrowser.Editor.Utils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    public class WorldBrowserWindow : EditorWindow {
        private const string REMOTE_TOOLBAR_STYLE_SHEET = "41113e2fd9468974386345f7119017a8";
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

        private StyleSheet remoteToolbarStyleSheet;
        private StyleSheet viewContainerStyleSheet;
        private StyleSheet hierarchySearchStyleSheet;
        private StyleSheet hierarchyStyleSheet;
        private StyleSheet inspectorStyleSheet;

        private Action<bool> OnRemoteStateChanged;
        private bool isApplicationPlaying;
        private bool isRemoteMode;
        private bool initialized;
        private bool stateChanged;

        [MenuItem("Tools/Morpeh/WorldBrowser")]
        private static void OpenWindow() {
            GetWindow<WorldBrowserWindow>();
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged += this.EditorApplicationOnPlayModeStateChanged;
            this.OnRemoteStateChanged += this.RemoteStateChanged;
            this.titleContent = new GUIContent("World Browser", Resources.Load<Texture>("MorpehIcons/64x64_W"), "Entities Browser");
            this.isApplicationPlaying = Application.isPlaying;
            this.LoadStyleSheets();
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= this.EditorApplicationOnPlayModeStateChanged;
            this.OnRemoteStateChanged -= this.RemoteStateChanged;
            this.inspectorView?.Dispose();
            this.modelsStorage?.Dispose();
        }

        private void LoadStyleSheets() {
            this.remoteToolbarStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(REMOTE_TOOLBAR_STYLE_SHEET);
            this.viewContainerStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(VIEW_CONTAINER_STYLE_SHEET);
            this.hierarchySearchStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(HIERARCHY_SEARCH_STYLE_SHEET);
            this.hierarchyStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(HIERARCHY_STYLE_SHEET);
            this.inspectorStyleSheet = AssetDatabaseUtility.LoadAssetWithGUID<StyleSheet>(INSPECTOR_STYLE_SHEET);
        }

        private void CreateGUI() {
            if (!this.isApplicationPlaying && !this.isRemoteMode) {
                this.rootVisualElement.Clear();
#if MORPEH_REMOTE_BROWSER
                var remoteToolbarView = new RemoteToolbar(this.OnRemoteStateChanged);
                this.rootVisualElement.styleSheets.Add(this.remoteToolbarStyleSheet);
                this.rootVisualElement.Add(remoteToolbarView);
#endif
                return;
            }

            if (this.initialized) {
                return;
            }

            this.modelsStorage = CreateModelsStorage();
            if (!this.modelsStorage.Initialize()) {
                this.isRemoteMode = false;
                return;
            }

            this.rootVisualElement.Clear();
            this.hierarchySearchViewModel = new HierarchySearchViewModel(this.modelsStorage.HierarchySearchProcessor, this.modelsStorage.ComponentStorageProcessor);
            this.hierarchyViewModel = new HierarchyViewModel(this.modelsStorage.HierarchyProcessor);
            this.inspectorViewModel = new InspectorViewModel(this.modelsStorage.InspectorProcessor, this.modelsStorage.ComponentStorageProcessor);

            this.splitterView = new SplitterView(250, 300);
            this.hierarchySearchView = new HierarchySearchView(this.hierarchySearchViewModel);
            this.hierarchyView = new HierarchyView(this.hierarchyViewModel);
            this.inspectorView = new InspectorView(this.inspectorViewModel);

            this.rootVisualElement.styleSheets.Add(this.viewContainerStyleSheet);
            this.rootVisualElement.styleSheets.Add(this.hierarchySearchStyleSheet);
            this.rootVisualElement.styleSheets.Add(this.hierarchyStyleSheet);
            this.rootVisualElement.styleSheets.Add(this.inspectorStyleSheet);

            this.splitterView.hierarchySearchRoot.Add(this.hierarchySearchView);
            this.splitterView.hierarchyRoot.Add(this.hierarchyView);
            this.splitterView.inspectorRoot.Add(this.inspectorView);

            this.rootVisualElement.Add(this.splitterView.root);
            this.initialized = true;
        }

        private void Update() {
            if (this.stateChanged) {
                this.Cleanup();
                this.CreateGUI();
                return;
            }

            if (this.modelsStorage == null && this.initialized) {
                this.Cleanup();
                this.CreateGUI();
                return;
            }

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

        private IModelsStorage CreateModelsStorage() {
#if MORPEH_REMOTE_BROWSER
            return isRemoteMode ? new Scellecs.Morpeh.WorldBrowser.Remote.RemoteModelsStorage(this.OnRemoteStateChanged) : new ModelsStorage();
#else
            return new ModelsStorage();
#endif
        }

        private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                if (!this.isRemoteMode) {
                    this.stateChanged = true;
                    this.isApplicationPlaying = false;
                }
            }
            else if (state == PlayModeStateChange.EnteredPlayMode) {
                if (!this.isRemoteMode) {
                    this.stateChanged = true;
                    this.isApplicationPlaying = true;
                }
            }
        }

        private void RemoteStateChanged(bool state) {
            this.stateChanged = true;
            this.isRemoteMode = state;
        }

        private void Cleanup() {
            this.inspectorView?.Dispose();
            this.modelsStorage?.Dispose();
            this.initialized = false;
            this.stateChanged = false;
        }
    }
}
#endif