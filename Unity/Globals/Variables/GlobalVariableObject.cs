namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;
    using Object = UnityEngine.Object;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Globals/Variable Object")]
    public class GlobalVariableObject : BaseGlobalVariable<Object> {
        public override IDataWrapper Wrapper {
            get => new ObjectWrapper {obj = this.value};
            set => this.Value = ((ObjectWrapper) value).obj;
        }
        
        public override bool CanBeAutoSaved => false;

        protected override Object Load(string serializedData) => JsonUtility.FromJson<ObjectWrapper>(serializedData).obj;

        protected override string Save() => JsonUtility.ToJson(new ObjectWrapper {obj = this.value});
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [Serializable]
        private class ObjectWrapper : IDataWrapper {
            public Object obj;
        }
    }
}