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
            get => new ListBigNumberWrapper {list = this.value};
            set => this.Value = ((ListBigNumberWrapper) value).list;
        }
        
        protected override List<BigNumber> Load(string serializedData) => JsonUtility.FromJson<ListBigNumberWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListBigNumberWrapper{list = this.value});

        [System.Serializable]
        private class ListBigNumberWrapper : IDataWrapper {
            public List<BigNumber> list;
        }
    }
}