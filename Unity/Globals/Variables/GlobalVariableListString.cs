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
        protected override List<string> Load(string serializedData)
            => JsonUtility.FromJson<ListStringWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListStringWrapper(this.value));
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    struct ListStringWrapper
    {
        public List<string> list;

        public ListStringWrapper(List<string> list)
        {
            this.list = list;
        }
    }
}