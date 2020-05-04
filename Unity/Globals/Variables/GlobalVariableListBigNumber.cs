namespace Morpeh.Globals {
    using System.Collections.Generic;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Morpeh.BigNumber;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Big Number")]
    public class GlobalVariableListBigNumber : BaseGlobalVariable<List<BigNumber>>
    {
        public override IDataWrapper Wrapper {
            get => new ListBigNumberWrapper(this.value);
            set => this.Value = ((ListBigNumberWrapper) value).ToBigNumberList();
        }
        
        protected override List<BigNumber> Load(string serializedData) => 
            JsonUtility.FromJson<ListBigNumberWrapper>(serializedData).ToBigNumberList();

        protected override string Save() {
            return JsonUtility.ToJson(new ListBigNumberWrapper (this.value));
        }
        
        internal override void OnEnable() {
            if (this.defaultValue != null)
            {
                this.value = ListBigNumberWrapper.ConvertToBigNumberList(this.defaultValue);
            }

            base.OnEnable();
        }
        
        [SerializeField]
        private List<string> defaultValue;
        public List<string> DefaultValue
        {
            private get => this.defaultValue;
            set => this.defaultValue = value;
        }

        [System.Serializable]
        private class ListBigNumberWrapper : IDataWrapper {
            public List<string> list;

            public ListBigNumberWrapper(List<BigNumber> bigList) {
                this.list = new List<string>();
                if(bigList == null) return;
                
                foreach (var big in bigList)
                {
                    this.list.Add(big.ToBigIntegerString());
                }
            }

            public List<BigNumber> ToBigNumberList() {
                return ConvertToBigNumberList(this.list);
            }

            public static List<BigNumber> ConvertToBigNumberList(List<string> list) {
                var result = new List<BigNumber>();
                foreach (var str in list)
                {
                    result.Add(BigNumber.Parse(str));
                }

                return result;
            }
        }
    }
}