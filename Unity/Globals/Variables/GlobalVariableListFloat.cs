namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable List Float")]
    public class GlobalVariableListFloat : BaseGlobalVariable<List<float>> {
        protected override List<float> Load(string serializedData)
            => JsonUtility.FromJson<ListFloatWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListFloatWrapper(this.value));
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Serializable]
    struct ListFloatWrapper
    {
        public List<float> list;

        public ListFloatWrapper(List<float> list)
        {
            this.list = list;
        }
    }
}