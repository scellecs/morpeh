namespace Morpeh.Globals {
    using System.Collections.Generic;
    using System.Linq;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Morpeh.BigNumber;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Big Number")]
    public class GlobalVariableListBigNumber : BaseGlobalVariable<List<BigNumber>>
    {
        public override DataWrapper Wrapper {
            get => new ListBigNumberWrapper {list = this.value};
            set => this.Value = ((ListBigNumberWrapper) value).list;
        }
        
        protected override List<BigNumber> Load(string serializedData) => JsonUtility.FromJson<ListBigNumberWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListBigNumberWrapper{list = this.value});
        
        public override string LastToString() => string.Join(",", this.BatchedChanges.Peek());

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [System.Serializable]
        private class ListBigNumberWrapper : DataWrapper {
            public List<BigNumber> list;
            
            public override string ToString() => string.Join(",", this.list);
        }
    }
}