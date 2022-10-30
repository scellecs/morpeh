namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Lists/Variable List Object")]
    public class GlobalVariableListObject : BaseGlobalVariable<List<Object>> {
        public override DataWrapper Wrapper {
            get => new ListObjectWrapper {list = this.value};
            set => this.Value = ((ListObjectWrapper) value).list;
        }
        
        public override bool CanBeAutoSaved => false;

        public override List<Object> Deserialize(string serializedData) => JsonUtility.FromJson<ListObjectWrapper>(serializedData).list;

        public override string Serialize(List<Object> data) => JsonUtility.ToJson(new ListObjectWrapper{list = data});

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ListObjectWrapper : DataWrapper {
            public List<Object> list;
            
            public override string ToString() => string.Join(",", this.list);
        }
    }
}