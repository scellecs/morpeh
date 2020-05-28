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
    [CreateAssetMenu(menuName = "ECS/Globals/Lists/Variable List Int")]
    public class GlobalVariableListInt : BaseGlobalVariable<List<int>> {
        public override DataWrapper Wrapper {
            get => new ListIntWrapper {list = this.value};
            set => this.Value = ((ListIntWrapper) value).list;
        }
        
        protected override List<int> Load(string serializedData)
            => JsonUtility.FromJson<ListIntWrapper>(serializedData).list;

        protected override string Save() => JsonUtility.ToJson(new ListIntWrapper {list = this.value});
        
        public override string LastToString() => string.Join(",", Format(this.BatchedChanges.Peek()));

        private static IEnumerable<string> Format(IEnumerable<int> list) => list.Select(i => i.ToString(CultureInfo.InvariantCulture));
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListIntWrapper : DataWrapper {
            public List<int> list;
            
            public override string ToString() => string.Join(",", Format(this.list));
        }
    }
}