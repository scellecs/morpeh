namespace Morpeh.Globals {
    using System;
    using JetBrains.Annotations;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using UnityEngine;
    using Object = UnityEngine.Object;

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
        private string defaultSerializedValue;

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
        }

        protected abstract TData Load([NotNull] string serializedData);
        protected abstract string Save();

        public virtual void Reset() {
            this.value = this.Load(this.defaultSerializedValue);
        }

        internal override void OnEnable() {
            base.OnEnable();
            this.defaultSerializedValue = this.Save();
            MApplicationFocusHook.OnApplicationFocusLost += this.SaveData;
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(this.customKey)) {
                this.GenerateCustomKey();
            }


            UnityEditor.EditorApplication.playModeStateChanged += this.EditorApplicationOnPlayModeStateChanged;
#endif
            this.LoadData();
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        [PropertyOrder(3)]
        [ShowIf("@AutoSave")]
        [HideInInlineEditors]
#endif
#if UNITY_EDITOR
        private void GenerateCustomKey() => this.customKey = Guid.NewGuid().ToString().Replace("-", string.Empty);
#endif
        internal override void OnDisable() {
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

        internal void SaveData() {
            if (this.AutoSave) {
                PlayerPrefs.SetString(this.Key, this.Save());
            }
        }

        #region EDITOR

#if UNITY_EDITOR
        private bool HasPlayerPrefsValue => PlayerPrefs.HasKey(this.Key);
        private bool HasPlayerPrefsValueAndAutoSave => PlayerPrefs.HasKey(this.Key) && this.AutoSave;


        private void EditorApplicationOnPlayModeStateChanged(UnityEditor.PlayModeStateChange state) {
            if (state == UnityEditor.PlayModeStateChange.EnteredEditMode) {
                this.SaveData();
                this.Reset();
                this.defaultSerializedValue = default;
                this.isLoaded = false;

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
        internal void ResetPlayerPrefsValue() {
            if (this.HasPlayerPrefsValue) {
                PlayerPrefs.DeleteKey(this.Key);
            }
        }
#endif

        #endregion
    }

    internal static class InitializerGlobalVariables {
        internal static MApplicationFocusHook hook;

        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeNoReturn() {
            var go = new GameObject("MORPEH__HOOK_APPLICATION_FOCUS");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            hook = go.AddComponent<MApplicationFocusHook>();
        }
    }

    internal class MApplicationFocusHook : MonoBehaviour {
        internal static Action OnApplicationFocusLost = () => { };

        internal void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                OnApplicationFocusLost.Invoke();
            }
        }
    }
}