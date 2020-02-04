namespace Morpeh.Unity.Tests.Runtime {
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Variables;

    [TestFixture]
    public class VariableTests {
        private const string ClassSO  = "Test Class Custom Variable";
        private const string StructSO = "Test Struct Custom Variable";

        [SetUp]
        public void SetUp() {
            var temp = Resources.Load<TestClassCustomVariable>(ClassSO);
            temp.ResetPlayerPrefsValue();
            temp.Value = default;
            Resources.UnloadAsset(temp);

            var temp2 = Resources.Load<TestStructCustomVariable>(StructSO);
            temp2.ResetPlayerPrefsValue();
            temp2.Value = default;
            Resources.UnloadAsset(temp2);
        }

        [TearDown]
        public void TearDown() {
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        [TestCase(0, 123, ExpectedResult  = null)]
        [TestCase(0, 456, ExpectedResult  = null)]
        [TestCase(0, 3213, ExpectedResult = null)]
        public IEnumerator SavingCustomVariableClass(int expectedValue, int newValue) {
            //Arrange
            var temp = Resources.Load<TestClassCustomVariable>(ClassSO);
            yield return null;

            //Act
            var t = temp.Value;
            t.clang  = newValue;
            t.cpp    = newValue;
            t.csharp = newValue;

            UnityRuntimeHelper.OnApplicationFocusLost();
            yield return null;
            temp.Reset();

            Resources.UnloadAsset(temp);
            temp = Resources.Load<TestClassCustomVariable>(ClassSO);
            yield return null;


            //Assert
            Assert.IsTrue(temp.AutoSave);
            Assert.AreEqual(expectedValue, temp.Value.clang);
            Assert.AreEqual(expectedValue, temp.Value.cpp);
            Assert.AreEqual(expectedValue, temp.Value.csharp);
            Assert.IsNotNull(temp);
        }

        [UnityTest]
        [TestCase(0, 123, ExpectedResult  = null)]
        [TestCase(0, 456, ExpectedResult  = null)]
        [TestCase(0, 3213, ExpectedResult = null)]
        public IEnumerator SavingCustomVariableStruct(int expectedValue, int newValue) {
            //Arrange
            var temp = Resources.Load<TestStructCustomVariable>(StructSO);
            yield return null;

            //Act
            var t = temp.Value;
            t.clang    = newValue;
            t.cpp      = newValue;
            t.csharp   = newValue;
            temp.Value = t;

            UnityRuntimeHelper.OnApplicationFocusLost();
            yield return null;
            temp.Reset();

            Resources.UnloadAsset(temp);
            temp = Resources.Load<TestStructCustomVariable>(StructSO);
            yield return null;


            //Assert
            Assert.IsTrue(temp.AutoSave);
            Assert.AreEqual(expectedValue, temp.Value.clang);
            Assert.AreEqual(expectedValue, temp.Value.cpp);
            Assert.AreEqual(expectedValue, temp.Value.csharp);
            Assert.IsNotNull(temp);
        }
    }
}