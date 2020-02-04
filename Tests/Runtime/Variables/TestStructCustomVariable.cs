namespace Morpeh.Unity.Tests.Runtime.Variables {
    using System;
    using Globals;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Globals/Custom/Tests/" + nameof(TestStructCustomVariable))]
    public class TestStructCustomVariable : BaseGlobalVariable<DummyStruct> {
        protected override DummyStruct Load(string serializedData) {
            this.value = JsonUtility.FromJson<DummyStruct>(serializedData);
            return this.value;
        }
        protected override string Save() => JsonUtility.ToJson(this.value);
    }

    [Serializable]
    public struct DummyStruct {
        public int clang;
        public int cpp;
        public int csharp;
    }
}