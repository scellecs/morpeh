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
        private const string WARNING_MSG = "Only for PlayMode";
        public override IDataWrapper Wrapper {
            get => new BigNumberWrapper {value = this.value};
            set => this.Value = ((BigNumberWrapper) value).value;
        }
        
        protected override BigNumber Load(string serializedData) {
            var result = BigNumber.Parse(serializedData);
            this.runtimeValue = result.ToString();
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

        internal override void OnEnable() {
            if(!string.IsNullOrEmpty(this.defaultValue)) 
                this.value.SetBigInteger(BigNumber.Parse(this.defaultValue));
            base.OnEnable();
        }

        [SerializeField]
        private string defaultValue;
        public string DefaultValue
        {
            private get => this.defaultValue;
            set => this.defaultValue = value;
        }
        
        [InlineButton("UpdateRuntimeValue")]
        [SerializeField]
        private string runtimeValue;
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        private void UpdateRuntimeValue() {
            if (!Application.isPlaying)
            {
                Debug.Log(WARNING_MSG);
                return;
            }
            this.value.SetBigInteger(BigNumber.Parse(this.runtimeValue));
            this.SaveData();
        }     
#endif
    }
}