namespace Morpeh.Globals {
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;
    using Morpeh.BigNumber;
    using System.Numerics;
    using Sirenix.OdinInspector;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Big Number")]
    public class GlobalVariableBigNumber : BaseGlobalVariable<BigNumber> {
        public override IDataWrapper Wrapper {
            get => new BigNumberWrapper {value = this.value};
            set => this.Value = ((BigNumberWrapper) value).value;
        }
        
        protected override BigNumber Load(string serializedData) {
            var result = BigNumber.Parse(serializedData);
            newValue = result.ToString();
            return result;
        }

        protected override string Save() {
            var str = this.value.ToBigIntegerString();
            return str;
        }

        [System.Serializable]
        private class BigNumberWrapper : IDataWrapper {
            public BigNumber value;
        }
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [InlineButton("UpdateValue")]
        [SerializeField]
        private string newValue;

        private void UpdateValue() {
            this.value.SetBigInteger(BigInteger.Parse(newValue));
            this.SaveData();
        }     
#endif
    }
}