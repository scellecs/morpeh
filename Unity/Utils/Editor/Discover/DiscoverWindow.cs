namespace Scellecs.Morpeh.Utils.Editor.Discover {
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class DiscoverWindow : EditorWindow {
        private const string KEY = "MORPEH_DISCOVER_SHOWATSTARTUP_";
        
        private static List<DiscoverAsset> startupDiscoverAssets;

        private static bool GetShowOnStartup(string name) => EditorPrefs.GetBool(KEY + name, true);

        private static void SetShowOnStartup(string name, bool value) {
            if (value != GetShowOnStartup(name)) {
                EditorPrefs.SetBool(KEY + name, value);
            }
        }

        public static void SelectDiscover(Discover discover) {
            foreach (var window in windows) {
                foreach (var categoryKvp in window.discoverObjects) {
                    if (categoryKvp.Value.Contains(discover)) {
                        window.SetSelectedDiscover(discover);
                        break;
                    }
                }
            }
        }

        private void OnEnable() {
            this.UpdateDiscoverObjects(true);
        }

        [InitializeOnLoadMethod]
        private static void InitShowAtStartup() {
            if (EditorApplication.timeSinceStartup <= 60) {
                var guids = AssetDatabase.FindAssets("t:DiscoverAsset");
                foreach (var guid in guids) {
                    var asset = AssetDatabase.LoadAssetAtPath<DiscoverAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    if (asset.EnableShowAtStartup) {
                        if (startupDiscoverAssets == null) {
                            startupDiscoverAssets = new List<DiscoverAsset>();
                        }

                        startupDiscoverAssets.Add(asset);
                    }
                }

                if (startupDiscoverAssets != null && startupDiscoverAssets.Count > 0) {
                    EditorApplication.delayCall += ShowAtStartup;
                }
            }
        }

        private static void ShowAtStartup() {
            if (!Application.isPlaying && startupDiscoverAssets != null) {
                foreach (var discoverAsset in startupDiscoverAssets) {
                    if (GetShowOnStartup(discoverAsset.PreferenceName)) {
                        ShowDiscoverWindow(discoverAsset);
                    }
                }
            }
        }
        

        private static List<DiscoverWindow> windows = default;

        public static void ShowDiscoverWindow(DiscoverAsset discoverAsset) {
            if (discoverAsset != null) {
                var window = GetWindow<DiscoverWindow>(!discoverAsset.dockable);
                window.SetDiscoverAsset(discoverAsset);
            }
            else {
                Debug.LogError("Could not open Discover Window : discoverAsset is null");
            }
        }

        
        public DiscoverAsset discoverAsset;
        private Texture2D     header;
        //private bool          forceGlobal;

        private void SetDiscoverAsset(DiscoverAsset discover) {
            this.discoverAsset = discover;
            this.titleContent  = new GUIContent(this.discoverAsset.WindowTitle);
        }

        private Dictionary<string, List<Discover>> discoverObjects = null;
        private Dictionary<string, bool> categoryFoldout = null;

        private void UpdateDiscoverObjects(bool clear = false) {
            if (this.discoverObjects == null) {
                this.discoverObjects = new Dictionary<string, List<Discover>>();
                this.categoryFoldout = new Dictionary<string, bool>();
            }

            if (clear) {
                this.discoverObjects.Clear();
            }

            var newOnes = AssetDatabase.FindAssets("t:Discover")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Discover>)
                .ToArray();

            // Add new ones
            foreach (var item in newOnes) {
                if (!this.discoverObjects.ContainsKey(item.Category)) {
                    this.discoverObjects.Add(item.Category, new List<Discover>());
                    this.categoryFoldout.Add(item.Category, false);
                }

                if (!this.discoverObjects[item.Category].Contains(item)) {
                    this.discoverObjects[item.Category].Add(item);
                }
            }

            // Cleanup Empty Entries
            var cleanedUpLists = new Dictionary<string, List<Discover>>();

            foreach (var categoryKvp in this.discoverObjects) {
                cleanedUpLists.Add(categoryKvp.Key, categoryKvp.Value.Where(o => o != null).ToList());
            }

            foreach (var categoryKvp in cleanedUpLists) {
                this.discoverObjects[categoryKvp.Key] = categoryKvp.Value;
            }

            // Cleanup Empty Categories
            var toDelete = new List<string>();
            foreach (var categoryKvp in this.discoverObjects) {
                if (categoryKvp.Value == null || categoryKvp.Value.Count == 0) {
                    toDelete.Add(categoryKvp.Key);
                }
            }

            foreach (var category in toDelete) {
                this.discoverObjects.Remove(category);
                this.categoryFoldout.Remove(category);
            }

            // Finally, sort items in each category
            foreach (var categoryKvp in this.discoverObjects) {
                this.discoverObjects[categoryKvp.Key].Sort((a, b) => { return Comparer<int>.Default.Compare(a.Priority, b.Priority); });
            }

            // Ensure something is selected is possible

            if (this.selectedDiscover == null) // Try Fetching a default
            {
                foreach (var categoryKvp in this.discoverObjects) {
                    this.selectedDiscover = categoryKvp.Value.FirstOrDefault(o => o.DefaultSelected == true);
                    if (this.selectedDiscover != null) {
                        break;
                    }
                }
            }

            if (this.selectedDiscover == null && this.discoverObjects != null && this.discoverObjects.Count > 0) {
                this.selectedDiscover = this.discoverObjects.First().Value.First();
            }

            this.Repaint();
        }

        private void OnGUI() {
            // Draw Header Image
            if (this.discoverAsset.HeaderTexture != null) {
                if (this.header == null || this.header != this.discoverAsset.HeaderTexture) {
                    this.header = this.discoverAsset.HeaderTexture;
                }

                var headerRect = GUILayoutUtility.GetRect(this.header.width, this.header.height);
                GUI.DrawTexture(headerRect, this.header);
            }
            else {
                var headerRect = GUILayoutUtility.GetRect(this.discoverAsset.WindowWidth, 80);
                EditorGUI.DrawRect(headerRect, new Color(0, 0, 0, 0.2f));
                headerRect.xMin += 16;
                headerRect.yMin += 16;
                GUI.Label(headerRect, this.discoverAsset.WindowTitle, Styles.header);
            }

            var hasContent = this.discoverObjects != null && this.discoverObjects.Count > 0;

            EditorGUI.EndDisabledGroup();

            if (hasContent) {
                this.SceneContentGUI();
            }

            // Draw Footer
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(this.discoverAsset.WindowWidth, 1), Color.black);
            using (new GUILayout.HorizontalScope()) {
                if (this.discoverAsset.EnableShowAtStartup) {
                    EditorGUI.BeginChangeCheck();
                    var showOnStartup = GUILayout.Toggle(GetShowOnStartup(this.discoverAsset.PreferenceName),
                        " Show this window on startup");
                    if (EditorGUI.EndChangeCheck()) {
                        SetShowOnStartup(this.discoverAsset.PreferenceName, showOnStartup);
                    }
                }

                GUILayout.FlexibleSpace();

                if (this.discoverAsset.Debug) {
                    if (GUILayout.Button("Select DiscoverAsset")) {
                        Selection.activeObject = this.discoverAsset;
                    }

                    if (GUILayout.Button("Reload")) {
                        this.UpdateDiscoverObjects(true);
                    }
                }

                if (GUILayout.Button("Close")) {
                    this.Close();
                }
            }
        }

        //private Vector2 globalContentScroll;

        private Discover selectedDiscover;
        private Vector2  listScroll;
        private Vector2  contentScroll;

        private void SceneContentGUI() {
            if (this.discoverObjects != null) {
                using (new GUILayout.HorizontalScope()) {
                    using (new GUILayout.VerticalScope()) {
                        this.listScroll = GUILayout.BeginScrollView(this.listScroll, GUI.skin.box,
                            GUILayout.Width(this.discoverAsset.DiscoverListWidth));
                        using (new GUILayout.VerticalScope(GUILayout.ExpandHeight(true))) {
                            foreach (var category in this.discoverObjects.Keys.OrderBy(x => x.ToString())) {
                                var show = true;
                                if (!string.IsNullOrEmpty(category)) {
                                    show = this.categoryFoldout[category] = EditorGUILayout.Foldout(this.categoryFoldout[category], category, EditorStyles.foldout);
                                }

                                if (show) {
                                    foreach (var item in this.discoverObjects[category]) {
                                        EditorGUI.BeginChangeCheck();
                                        var value = GUILayout.Toggle(item == this.selectedDiscover, item.Name, Styles.listItem);

                                        if (value) {
                                            // Select the new one if not selected
                                            if (this.selectedDiscover != item) {
                                                if (EditorGUI.EndChangeCheck()) {
                                                    if (this.discoverAsset.Debug) {
                                                        Selection.activeObject = item;
                                                    }

                                                    this.SetSelectedDiscover(item);
                                                }
                                            }

                                            var r = GUILayoutUtility.GetLastRect();
                                            var c = EditorGUIUtility.isProSkin ? 1 : 0;
                                            EditorGUI.DrawRect(r, new Color(c, c, c, 0.1f));
                                        }
                                    }
                                }
                            }

                            GUILayout.FlexibleSpace();
                        }

                        GUILayout.EndScrollView();
                    }

                    GUILayout.Space(4);

                    using (new GUILayout.VerticalScope()) {
                        this.contentScroll = GUILayout.BeginScrollView(this.contentScroll);
                        GUILayout.Space(8);

                        DiscoverEditor.DrawDiscoverContentGUI(this.selectedDiscover);

                        GUILayout.FlexibleSpace();
                        GUILayout.EndScrollView();
                    }
                }
            }
            else {
                this.UpdateDiscoverObjects();
            }
        }

        private void SetSelectedDiscover(Discover newSelection) {
            // Set the new item
            this.selectedDiscover = newSelection;
            this.contentScroll    = Vector2.zero;
        }

        public class GroupLabelScope : GUILayout.VerticalScope {
            public GroupLabelScope(string name) : base(Styles.box) {
                if (!string.IsNullOrWhiteSpace(name)) {
                    var s = Styles.boxHeader;
                    var n = new GUIContent(name);
                    var r = GUILayoutUtility.GetRect(n, Styles.boxHeader, GUILayout.ExpandWidth(true));
                    GUI.Label(r, n, Styles.boxHeader);
                }
            }
        }

        public static class Styles {
            public static GUIStyle indent;
            public static GUIStyle slightIndent;

            public static GUIStyle header;
            public static GUIStyle subHeader;
            public static GUIStyle body;

            public static GUIStyle box;
            public static GUIStyle boxHeader;

            public static GUIStyle listItem;

            public static GUIStyle buttonLeft;
            public static GUIStyle buttonMid;
            public static GUIStyle buttonRight;

            public static GUIStyle tabContainer;

            public static GUIStyle image;

            static Styles() {
                header          = new GUIStyle(EditorStyles.wordWrappedLabel);
                header.fontSize = 24;
                header.padding  = new RectOffset(0, 0, -4, -4);
                header.richText = true;

                subHeader           = new GUIStyle(EditorStyles.wordWrappedLabel);
                subHeader.fontSize  = 11;
                subHeader.fontStyle = FontStyle.Italic;

                body          = new GUIStyle(EditorStyles.wordWrappedLabel);
                body.fontSize = 11;
                body.richText = true;

                indent         = new GUIStyle();
                indent.padding = new RectOffset(12, 12, 12, 12);

                slightIndent         = new GUIStyle();
                slightIndent.padding = new RectOffset(6, 6, 0, 6);

                box = new GUIStyle(EditorStyles.helpBox);

                boxHeader                  = new GUIStyle(GUI.skin.box);
                boxHeader.normal.textColor = GUI.skin.label.normal.textColor;
                boxHeader.fixedHeight      = 27;
                boxHeader.fontSize         = 16;
                boxHeader.fontStyle        = FontStyle.Bold;
                boxHeader.alignment        = TextAnchor.UpperLeft;
                boxHeader.margin           = new RectOffset(0, 0, 0, 6);

                listItem         = new GUIStyle(EditorStyles.label);
                listItem.padding = new RectOffset(12, 0, 2, 2);

                buttonLeft           = new GUIStyle(EditorStyles.miniButtonLeft);
                buttonLeft.fontSize  = 11;
                buttonMid            = new GUIStyle(EditorStyles.miniButtonMid);
                buttonMid.fontSize   = 11;
                buttonRight          = new GUIStyle(EditorStyles.miniButtonRight);
                buttonRight.fontSize = 11;

                tabContainer         = new GUIStyle(EditorStyles.miniButton);
                tabContainer.padding = new RectOffset(4, 4, 0, 0);

                image              = new GUIStyle(GUIStyle.none);
                image.stretchWidth = true;
            }
        }
    }
}