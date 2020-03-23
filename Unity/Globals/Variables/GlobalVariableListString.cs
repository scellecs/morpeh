namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List String")]
    public class GlobalVariableListString : BaseGlobalVariable<List<string>> {
        public override IDataWrapper Wrapper {
            get => new ListStringWrapper {list = this.value};
            set => this.Value = ((ListStringWrapper) value).list;
        }
        
        protected override List<string> Load(string serializedData)
            => JsonUtility.FromJson<ListStringWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListStringWrapper {list = this.value});

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListStringWrapper : IDataWrapper {
            public List<string> list;
        }
    }
}