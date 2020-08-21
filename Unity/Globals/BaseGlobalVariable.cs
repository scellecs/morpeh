namespace Morpeh.Globals {
    using System;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class DataWrapper {
        public abstract override string ToString();
    }

    public interface IDataVariable {
        DataWrapper Wrapper { get; set; }
    }

    public abstract class BaseGlobalVariable<TData> : BaseGlobalEvent<TData>, IDataVariable {
        [Space]
        [Header("Runtime Data")]
        [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(10)]
        [OnValueChanged(nameof(OnChange))]
        [DelayedProperty]
        [HideLabel]
#endif
        protected TData value;
        [HideInInspector]
        [SerializeField]
        private string defaultSerializedValue;
        private const string COMMON_KEY = "MORPEH__GLOBALS_VARIABLES_";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [PropertyOrder(1)]
        [ShowIf("@" + nameof(AutoSave) + " && " + nameof(CanBeAutoSaved))]
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

        public virtual bool CanBeAutoSaved => true;
        [Header("Saving Settings")]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [PropertyOrder(0)]
        [ShowIf(nameof(CanBeAutoSaved))]
#endif
        public bool AutoSave;
        private bool HasPlayerPrefsValue            => PlayerPrefs.HasKey(this.Key);
        private bool HasPlayerPrefsValueAndAutoSave => PlayerPrefs.HasKey(this.Key) && this.AutoSave;
        private bool isLoaded;

        public abstract DataWrapper Wrapper { get; set; }

        public TData Value {
            get => this.value;
            set => this.SetValue(value);
        }

        public void SetValue(TData newValue) {
            this.value = newValue;
            this.OnChange(newValue);
        }
        
        public void SetValueNextFrame(TData newValue) {
            this.value = newValue;
            this.OnChangeNextFrame(newValue);
        }
        
        private void OnChange() {
            this.OnChange(this.value);
        }
        
        private void OnChange(TData newValue) {
            if (Application.isPlaying) {
                this.CheckIsInitialized();
                this.Publish(newValue);
            }
        }
        
        private void OnChangeNextFrame(TData newValue) {
            if (Application.isPlaying) {
                this.CheckIsInitialized();
                this.NextFrame(newValue);
            }
        }

        public virtual void Reset() {
            if (!string.IsNullOrEmpty(this.defaultSerializedValue)) {
                this.value = this.Deserialize(this.defaultSerializedValue);
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.__internalKey = null;
            UnityRuntimeHelper.onApplicationFocusLost += this.SaveData;
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(this.customKey)) {
                this.GenerateCustomKey();
            }
#endif
            this.LoadData();
        }
#if UNITY_EDITOR
        protected override void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            base.OnEditorApplicationOnplayModeStateChanged(state);
            if (state == PlayModeStateChange.EnteredEditMode) {
                this.SaveData();
                this.Reset();
                this.defaultSerializedValue = default;
                this.isLoaded               = false;
            }
            else if (state == PlayModeStateChange.ExitingEditMode) {
                this.LoadData();
            }
        }
#endif
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        [PropertyOrder(3)]
        [ShowIf("@AutoSave")]
        [HideInInlineEditors]
#endif
#if UNITY_EDITOR
        private void GenerateCustomKey() {
            this.__internalKey = null;
            this.customKey = Guid.NewGuid().ToString().Replace("-", string.Empty);
        } 
#endif
        public override void Dispose() {
            base.Dispose();
            UnityRuntimeHelper.onApplicationFocusLost -= this.SaveData;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            this.SaveData();
            this.isLoaded = false;
        }

        private void LoadData() {
#if UNITY_EDITOR
            try {
#endif
    #if UNITY_EDITOR
                if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                    return;
                }
    #endif
                
                if (this.isLoaded) {
                    return;
                }

                this.defaultSerializedValue = this.Serialize(this.value);
                this.isLoaded = true;
                
                if (!this.AutoSave) {
                    return;
                }
                if (!PlayerPrefs.HasKey(this.Key)) {
                    return;
                }

                this.value = this.Deserialize(PlayerPrefs.GetString(this.Key));
#if UNITY_EDITOR
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
#endif
        }

        internal void SaveData() {
#if UNITY_EDITOR
            try {
#endif
                if (this.AutoSave) {
                    PlayerPrefs.SetString(this.Key, this.Serialize(this.value));
                }
#if UNITY_EDITOR
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
#endif
        }

        #region EDITOR

#if UNITY_EDITOR
#if UNITY_EDITOR && ODIN_INSPECTOR
        [HideInInlineEditors]
        [ShowIf("@" + nameof(HasPlayerPrefsValueAndAutoSave))]
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
}