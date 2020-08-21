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
    [CreateAssetMenu(menuName = "ECS/Globals/Lists/Variable List Bool")]
    public class GlobalVariableListBool : BaseGlobalVariable<List<bool>> {
        public override DataWrapper Wrapper {
            get => new ListBoolWrapper {list = this.value};
            set => this.Value = ((ListBoolWrapper) value).list;
        }
        
        public override List<bool> Deserialize(string serializedData) => JsonUtility.FromJson<ListBoolWrapper>(serializedData).list;

        public override string Serialize(List<bool> data) => JsonUtility.ToJson(new ListBoolWrapper {list = data});
        
        private static IEnumerable<string> Format(IEnumerable<bool> list) => list.Select(i => i.ToString(CultureInfo.InvariantCulture));

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListBoolWrapper : DataWrapper {
            public List<bool> list;
            
            public override string ToString() => string.Join(",", Format(this.list));
        }
    }
}