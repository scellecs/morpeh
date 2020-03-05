namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Bool")]
    public class GlobalVariableListBool : BaseGlobalVariable<List<bool>> {
        protected override List<bool> Load(string serializedData)
            => JsonUtility.FromJson<ListBoolWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListBoolWrapper {list = this.value});

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private struct ListBoolWrapper {
            public List<bool> list;
        }
    }
}