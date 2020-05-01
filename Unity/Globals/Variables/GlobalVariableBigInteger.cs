using System.Numerics;
using Morpeh.BigInt;
using Sirenix.OdinInspector;

namespace Morpeh.Globals {
    using System.Globalization;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Big Integer")]
    public class GlobalVariableBigInteger : BaseGlobalVariable<BigInt.BigInt> {
        public override IDataWrapper Wrapper {
            get => new BigIntegerWrapper {value = this.value};
            set => this.Value = ((BigIntegerWrapper) value).value;
        }
        
        protected override BigInt.BigInt Load(string serializedData) {
            var result = BigInt.BigInt.Parse(serializedData);
            newValue = result.ToString();
            return result;
        }

        protected override string Save() {
            var str = this.value.ToBigIntegerString();
            return str;
        }

        [System.Serializable]
        private class BigIntegerWrapper : IDataWrapper {
            public BigInt.BigInt value;
        }
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [InlineButton("UpdateValue")]
        [SerializeField]
        private string newValue;

        private void UpdateValue() {
            this.value.SetBitInteger(BigInteger.Parse(newValue));
            this.SaveData();
        }     
#endif
    }
}