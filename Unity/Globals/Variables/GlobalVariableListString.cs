namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Lists/Variable List String")]
    public class GlobalVariableListString : BaseGlobalVariable<List<string>> {
        public override DataWrapper Wrapper {
            get => new ListStringWrapper {list = this.value};
            set => this.Value = ((ListStringWrapper) value).list;
        }
        
        public override List<string> Deserialize(string serializedData) => JsonUtility.FromJson<ListStringWrapper>(serializedData).list;

        public override string Serialize(List<string> data) => JsonUtility.ToJson(new ListStringWrapper {list = data});

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListStringWrapper : DataWrapper {
            public List<string> list;
            
            public override string ToString() => string.Join(",", this.list);
        }
    }
}