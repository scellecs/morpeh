namespace Morpeh.Globals {
    using System;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variables/Variable Color")]
    public class GlobalVariableColor : BaseGlobalVariable<Color> {
        public override DataWrapper Wrapper {
            get => new ColorWrapper {value = this.value};
            set => this.Value = ((ColorWrapper) value).value;
        }
        
        public override bool CanBeAutoSaved => false;

        protected override Color Load(string serializedData) => JsonUtility.FromJson<ColorWrapper>(serializedData).value;

        protected override string Save() => JsonUtility.ToJson(new ColorWrapper() {value = this.value});
        
        public override string LastToString() => this.BatchedChanges.Peek().ToString();
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ColorWrapper : DataWrapper {
            public Color value;
            
            public override string ToString() => this.value.ToString();
        }
    }
}