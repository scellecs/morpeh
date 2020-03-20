namespace Morpeh.Globals {
    using System;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public interface IDataWrapper {
        
    }
    
    public interface IDataVariable {
        IDataWrapper Wrapper { get; set; }
    }
    
    public abstract class BaseGlobalVariable<TData> : BaseGlobalEvent<TData>, IDataVariable {
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

        public abstract IDataWrapper Wrapper { get; set; }
        
        public TData Value {
            get {
                if (!this.isLoaded) {
                    this.defaultSerializedValue = this.Save();
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

        protected abstract TData  Load([NotNull] string serializedData);
        protected abstract string Save();

        public virtual void Reset() {
            if (!string.IsNullOrEmpty(this.defaultSerializedValue)) {
                this.value = this.Load(this.defaultSerializedValue);
            }
        }

        internal override void OnEnable() {
            base.OnEnable();
            this.defaultSerializedValue               =  this.Save();
            UnityRuntimeHelper.OnApplicationFocusLost += this.SaveData;
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(this.customKey)) {
                this.GenerateCustomKey();
            }
#else
            this.LoadData();
#endif
        }
#if UNITY_EDITOR
        internal override void OnEditorApplicationOnplayModeStateChanged(PlayModeStateChange state) {
            base.OnEditorApplicationOnplayModeStateChanged(state);
            if (state == PlayModeStateChange.EnteredEditMode) {
                this.SaveData();
                this.Reset();
                this.defaultSerializedValue = default;
                this.isLoaded               = false;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode) {
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
        private void GenerateCustomKey() => this.customKey = Guid.NewGuid().ToString().Replace("-", string.Empty);
#endif
        public override void Dispose() {
            base.Dispose();
            UnityRuntimeHelper.OnApplicationFocusLost -= this.SaveData;
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
        }

        internal void SaveData() {
            if (this.AutoSave) {
                PlayerPrefs.SetString(this.Key, this.Save());
            }
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