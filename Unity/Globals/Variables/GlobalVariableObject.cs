namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;
    using Object = UnityEngine.Object;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variables/Variable Object")]
    public class GlobalVariableObject : BaseGlobalVariable<Object> {
        public override DataWrapper Wrapper {
            get => new ObjectWrapper {value = this.value};
            set => this.Value = ((ObjectWrapper) value).value;
        }
        
        public override bool CanBeAutoSaved => false;

        protected override Object Load(string serializedData) => JsonUtility.FromJson<ObjectWrapper>(serializedData).value;

        protected override string Save() => JsonUtility.ToJson(new ObjectWrapper {value = this.value});
        
        public override string LastToString() => this.BatchedChanges.Peek().ToString();
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ObjectWrapper : DataWrapper {
            public Object value;
            
            public override string ToString() => this.value.ToString();
        }
    }
}