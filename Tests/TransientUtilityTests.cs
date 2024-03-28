/*
using Scellecs.Morpeh;

namespace Tests;

public class TransientUtilityTests {
    [Fact]
    public void AddComponentWorks() {
        var transient = new TransientArchetype();
        TransientUtility.Initialize(ref transient);

        for (var i = 0; i < 64; i++) {
            var typeInfo = new TypeInfo(new TypeOffset(i), new TypeId(i));
            TransientUtility.AddComponent(ref transient, ref typeInfo);
        }

        Assert.Equal(64, transient.changesCount);

        for (var i = 0; i < 64; i++) {
            Assert.Equal(i, transient.changes[i].typeOffset.GetValue());
            Assert.True(transient.changes[i].isAddition);
        }
    }

    [Fact]
    public void RemoveComponentWorks() {
        var transient = new TransientArchetype();
        TransientUtility.Initialize(ref transient);

        for (var i = 0; i < 64; i++) {
            var typeInfo = new TypeInfo(new TypeOffset(i), new TypeId(i));
            TransientUtility.RemoveComponent(ref transient, ref typeInfo);
        }

        Assert.Equal(64, transient.changesCount);

        for (var i = 0; i < 64; i++) {
            Assert.Equal(i, transient.changes[i].typeOffset.GetValue());
            Assert.False(transient.changes[i].isAddition);
        }
    }

    [Fact]
    public void RebaseClears() {
        var transient = new TransientArchetype();
        TransientUtility.Initialize(ref transient);

        for (var i = 0; i < 64; i++) {
            var typeInfo = new TypeInfo(new TypeOffset(i), new TypeId(i));
            TransientUtility.AddComponent(ref transient, ref typeInfo);
            
            Assert.Equal(i + 1, transient.changesCount);
        }
        
        TransientUtility.Rebase(ref transient, null);
        
        Assert.Equal(ArchetypeId.Invalid, transient.nextArchetypeId);
        Assert.Equal(0, transient.changesCount);
        Assert.Null(transient.baseArchetype);
    }
}
*/