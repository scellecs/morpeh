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
        public override IDataWrapper Wrapper {
            get => new ListFloatWrapper {list = this.value};
            set => this.Value = ((ListFloatWrapper) value).list;
        }

        protected override List<float> Load(string serializedData)
            => JsonUtility.FromJson<ListFloatWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListFloatWrapper {list = this.value});

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListFloatWrapper : IDataWrapper {
            public List<float> list;
        }
    }
}