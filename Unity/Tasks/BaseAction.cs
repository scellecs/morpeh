namespace Morpeh.Tasks {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
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