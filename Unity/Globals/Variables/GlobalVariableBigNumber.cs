namespace Morpeh.Globals {
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;
    using BigNumber;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variables/Variable Big Number")]
    public class GlobalVariableBigNumber : BaseGlobalVariable<BigNumber> {
#if UNITY_EDITOR
        private const string WARNING_MSG = "Only for PlayMode";
#endif
        public override DataWrapper Wrapper {
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

        public override string LastToString() => this.BatchedChanges.Peek().ToString();

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [System.Serializable]
        private class BigNumberWrapper : DataWrapper {
            public BigNumber value;
            
            public override string ToString() => this.value.ToString();
        }

        protected override void OnEnable() {
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
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [InlineButton("UpdateRuntimeValue")]
#endif
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