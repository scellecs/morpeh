namespace Morpeh.Globals {
    using System;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using UnityEngine;

    public abstract class BaseGlobalVariable<TData> : BaseGlobalEvent<TData> {
        [Space]
        [Header("Runtime Data")]
        [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(10)]
        [OnValueChanged("OnChange")]
        [DelayedProperty]
        [HideLabel]
#endif
        protected TData value;

        private TData lastValue;

        private const string COMMON_KEY = "MORPEH__GLOBALS_VARIABLES_";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [PropertyOrder(1)]
        [ShowIf("@AutoSave")]
#endif
        [SerializeField]
        private string customKey;

        // ReSharper disable once InconsistentNaming
        private string __internalKey;

        private string Key {
            get {
                if (string.IsNullOrEmpty(this.__internalKey)) {
                    this.__internalKey = COMMON_KEY + this.customKey;
                }

                return this.__internalKey;
            }
        }

        [Header("Saving Settings")]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [PropertyOrder(0)]
#endif
        public bool AutoSave;

        private bool isLoaded;

        public TData Value {
            get {
                if (!this.isLoaded) {
                    this.LoadData();
                    this.isLoaded = true;
                }

                return this.value;
            }
            set => this.SetValue(value);
        }

        private void SetValue(TData newValue) {
            this.value = newValue;
            this.OnChange(newValue);
        }

        private void OnChange(TData newValue) {
            if (Application.isPlaying) {
                this.CheckIsInitialized();
                this.Publish(newValue);
                this.SaveData();
            }
#if UNITY_EDITOR
            else {
                this.editorBackfield = this.Save();
            }
#endif
        }

        protected abstract TData  Load(string serializedData);
        protected abstract string Save();

        protected override void OnEnable() {
            base.OnEnable();
            MApplicationFocusHook.OnApplicationFocusLost += this.SaveData;
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(this.customKey)) {
                this.GenerateCustomKey();
            }

            this.editorBackfield = this.Save();

            UnityEditor.EditorApplication.playModeStateChanged += this.EditorApplicationOnPlayModeStateChanged;
#endif
            this.LoadData();
        }
#if UNITY_EDITOR
        //[Button]
        //[PropertyOrder(3)]
        private void GenerateCustomKey() => this.customKey = Guid.NewGuid().ToString().Replace("-", string.Empty);
#endif
        protected override void OnDisable() {
            base.OnDisable();
            MApplicationFocusHook.OnApplicationFocusLost -= this.SaveData;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            this.SaveData();
        }

        private void LoadData() {
            if (!this.AutoSave) {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            if (!PlayerPrefs.HasKey(this.Key)) {
                return;
            }

            this.value = this.Load(PlayerPrefs.GetString(this.Key));
            this.OnChange(this.value);
        }

        private void SaveData() {
            if (this.AutoSave) {
                PlayerPrefs.SetString(this.Key, this.Save());
            }
        }

        #region EDITOR

#if UNITY_EDITOR
        //hack for save start values of GlobalVariables
        private string editorBackfield;

        private bool HasPlayerPrefsValue            => PlayerPrefs.HasKey(this.Key);
        private bool HasPlayerPrefsValueAndAutoSave => PlayerPrefs.HasKey(this.Key) && this.AutoSave;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [PropertyOrder(2)]
        [ShowInInspector]
        [ShowIf("@HasPlayerPrefsValueAndAutoSave")]
#endif
        private TData PlayerPrefsValue {
            get {
                if (this.HasPlayerPrefsValue) {
                    return this.Load(PlayerPrefs.GetString(this.Key));
                }

                return default;
            }
        }


        private void EditorApplicationOnPlayModeStateChanged(UnityEditor.PlayModeStateChange state) {
            if (state == UnityEditor.PlayModeStateChange.EnteredEditMode) {
                this.SaveData();
                this.value           = this.Load(this.editorBackfield);
                this.editorBackfield = default;
                this.isLoaded        = false;

                UnityEditor.EditorApplication.playModeStateChanged -= this.EditorApplicationOnPlayModeStateChanged;
            }
            else if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode) {
                this.LoadData();
            }
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [ShowIf("@HasPlayerPrefsValueAndAutoSave")]
        [PropertyOrder(4)]
        [Button]
#endif
        private void ResetPlayerPrefsValue() {
            if (this.HasPlayerPrefsValue) {
                PlayerPrefs.DeleteKey(this.Key);
            }
        }
#endif

        #endregion
    }

    internal static class InitializerGlobalVariables {
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize() {
            var go = new GameObject("MORPEH__HOOK_APPLICATION_FOCUS");
            go.AddComponent<MApplicationFocusHook>();
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }

    internal class MApplicationFocusHook : MonoBehaviour {
        internal static Action OnApplicationFocusLost = () => { };

        private void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                OnApplicationFocusLost.Invoke();
            }
        }
    }
}