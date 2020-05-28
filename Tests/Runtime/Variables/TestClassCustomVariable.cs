namespace Morpeh.Unity.Tests.Runtime.Variables {
    using System;
    using Globals;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Globals/Custom/Tests/" + nameof(TestClassCustomVariable))]
    public class TestClassCustomVariable : BaseGlobalVariable<DummyClass> {
        public override DataWrapper Wrapper { get; set; }

        protected override DummyClass Load(string serializedData) {
            if (!string.IsNullOrEmpty(serializedData)) {
                if (this.value != null) {
                    JsonUtility.FromJsonOverwrite(serializedData, this.value);
                }
                else {
                    this.value = JsonUtility.FromJson<DummyClass>(serializedData);
                }
            }
            return this.value;
        }
        protected override string Save() => JsonUtility.ToJson(this.value);
        
        public override string LastToString() => this.BatchedChanges.Peek().ToString();
    }

    [Serializable]
    public class DummyClass {
        public int clang;
        public int cpp;
        public int csharp;
    }
}