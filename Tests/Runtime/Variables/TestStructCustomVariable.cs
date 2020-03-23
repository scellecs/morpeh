namespace Morpeh.Unity.Tests.Runtime.Variables {
    using System;
    using Globals;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Globals/Custom/Tests/" + nameof(TestStructCustomVariable))]
    public class TestStructCustomVariable : BaseGlobalVariable<DummyStruct> {
        public override IDataWrapper Wrapper { get; set; }

        protected override DummyStruct Load(string serializedData) => JsonUtility.FromJson<DummyStruct>(serializedData);
        
        protected override string Save() => JsonUtility.ToJson(this.value);
    }

    [Serializable]
    public struct DummyStruct {
        public int clang;
        public int cpp;
        public int csharp;
    }
}