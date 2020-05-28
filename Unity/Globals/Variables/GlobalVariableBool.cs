namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variables/Variable Bool")]
    public class GlobalVariableBool : BaseGlobalVariable<bool> {
        public override DataWrapper Wrapper {
            get => new BoolWrapper {value = this.value};
            set => this.Value = ((BoolWrapper) value).value;
        }

        protected override bool Load(string serializedData) => bool.Parse(serializedData);

        protected override string Save() => this.value.ToString(CultureInfo.InvariantCulture);
        
        public override string LastToString() => this.BatchedChanges.Peek().ToString(CultureInfo.InvariantCulture);

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [System.Serializable]
        private class BoolWrapper : DataWrapper {
            public bool value;
            public override string ToString() => this.value.ToString(CultureInfo.InvariantCulture);
        }
    }
}