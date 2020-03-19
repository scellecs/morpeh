namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Float")]
    public class GlobalVariableFloat : BaseGlobalVariable<float> {
        public override IDataWrapper Wrapper {
            get => new FloatWrapper {value = this.value};
            set => this.Value = ((FloatWrapper) value).value;
        }

        protected override float Load(string serializedData) => float.Parse(serializedData, CultureInfo.InvariantCulture);

        protected override string Save() => this.value.ToString(CultureInfo.InvariantCulture);
        
        [System.Serializable]
        private class FloatWrapper : IDataWrapper {
            public float value;
        }
    }
}