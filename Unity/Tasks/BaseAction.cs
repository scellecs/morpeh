namespace Morpeh.Tasks {
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class BaseAction : ScriptableObject {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [DelayedProperty]
        [ShowInInspector]
        [HideInInlineEditors]
        private string actionName {
            get => this.name;
            set {
                this.name = value;
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
#endif

        public abstract void Execute();
    }
}