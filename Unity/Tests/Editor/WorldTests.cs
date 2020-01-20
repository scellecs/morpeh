namespace Morpeh.Unity.Tests.Editor {
    using NUnit.Framework;
    using Runtime;

    [TestFixture(Category = "Morpeh/World")]
    public class WorldTests {
        protected World World;
        
        [SetUp]
        public void Setup() => this.World = new World();

        [TearDown]
        public void TearDown() {
            this.World.Dispose();
            this.World = null;
        }
        
        [Test]
        public void World_Entities_Capacity_Is_Same_As_Const_Capacity_By_Default() {
            Assert.AreEqual(this.World.EntitiesCapacity,Constants.DEFAULT_WORLD_ENTITIES_CAPACITY);
        }
        
        [Test]
        public void World_Entities_Length_Is_Same_As_Capacity_By_Default() {
            Assert.AreEqual(this.World.EntitiesCapacity,this.World.Entities.Length);
        }
        
        [Test]
        public void World_Entities_Public_Length_Is_Zero_By_Default() {
            Assert.AreEqual(this.World.EntitiesLength,0);
        }
        
        [Test]
        [TestCase(1)]
        [TestCase(12)]
        [TestCase(123)]
        [TestCase(1234)]
        public void World_Entities_Public_Length_Is_Grow_Right(int countEntities) {
            for (int i = 0, length = countEntities; i < length; i++) {
                this.World.CreateEntity();
            }
            Assert.AreEqual(this.World.EntitiesLength,countEntities);
        }
        
        [Test]
        [TestCase(Constants.DEFAULT_WORLD_ENTITIES_CAPACITY + 1, Constants.DEFAULT_WORLD_ENTITIES_CAPACITY * 2)]
        [TestCase(Constants.DEFAULT_WORLD_ENTITIES_CAPACITY * 2 + 1, (Constants.DEFAULT_WORLD_ENTITIES_CAPACITY * 2) * 2)]
        public void World_Entities_Capacity_Is_Grow_Right(int countEntities, int expected) {
            for (int i = 0, length = countEntities; i < length; i++) {
                this.World.CreateEntity();
            }
            Assert.AreEqual(this.World.Entities.Length,expected);
            Assert.AreEqual(this.World.EntitiesCapacity,expected);
        }
    }
}