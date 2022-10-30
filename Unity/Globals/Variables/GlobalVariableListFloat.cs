namespace Morpeh.Globals {
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Globalization;
    using System.Linq;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Lists/Variable List Float")]
    public class GlobalVariableListFloat : BaseGlobalVariable<List<float>> {
        public override DataWrapper Wrapper {
            get => new ListFloatWrapper {list = this.value};
            set => this.Value = ((ListFloatWrapper) value).list;
        }

        public override List<float> Deserialize(string serializedData)
            => JsonUtility.FromJson<ListFloatWrapper>(serializedData).list;

        public override string Serialize(List<float> data) => JsonUtility.ToJson(new ListFloatWrapper {list = data});

        private static IEnumerable<string> Format(IEnumerable<float> list) => list.Select(i => i.ToString(CultureInfo.InvariantCulture));

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListFloatWrapper : DataWrapper {
            public List<float> list;
            public override string ToString() => string.Join(",",Format(this.list));
        }
    }
}