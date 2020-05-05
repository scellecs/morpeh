namespace Morpeh.Globals {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Morpeh.BigNumber;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Big Number")]
    public class GlobalVariableBigNumber : BaseGlobalVariable<BigNumber>
    {
        public override DataWrapper Wrapper {
            get => new BigNumberWrapper {value = this.value};
            set => this.Value = ((BigNumberWrapper) value).value;
        }

        protected override BigNumber Load(string serializedData) => JsonUtility.FromJson<BigNumber>(serializedData);

        protected override string Save() => JsonUtility.ToJson(this.value);

        public override string LastToString() => this.BatchedChanges.Peek().ToString();

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [System.Serializable]
        private class BigNumberWrapper : DataWrapper {
            public BigNumber value;
            
            public override string ToString() => this.value.ToString();
        }
    }
}