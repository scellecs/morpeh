using Scellecs.Morpeh.Collections;

namespace Tests;

public class LongHashSetTests {
    [Fact]
    public void SanityTest() {
        var longHashSet = new LongHashSet();
        Assert.Equal(0, longHashSet.length);
        
        longHashSet.Add(77777721321231L);
        Assert.Equal(1, longHashSet.length);
        Assert.True(longHashSet.Has(77777721321231L));
        
        longHashSet.Add(77777721321231L);
        Assert.Equal(1, longHashSet.length);
        Assert.True(longHashSet.Has(77777721321231L));
        
        longHashSet.Add(77777721321232L);
        Assert.Equal(2, longHashSet.length);
        Assert.True(longHashSet.Has(77777721321232L));
        
        longHashSet.Add(77777721321233L);
        Assert.Equal(3, longHashSet.length);
        Assert.True(longHashSet.Has(77777721321233L));
        
        longHashSet.Remove(77777721321231L);
        Assert.Equal(2, longHashSet.length);
        Assert.False(longHashSet.Has(77777721321231L));
        
        longHashSet.Remove(77777721321231L);
        Assert.Equal(2, longHashSet.length);
        Assert.False(longHashSet.Has(77777721321231L));
        
        longHashSet.Remove(77777721321232L);
        Assert.Equal(1, longHashSet.length);
        Assert.False(longHashSet.Has(77777721321232L));
        
        longHashSet.Remove(77777721321233L);
        Assert.Equal(0, longHashSet.length);
        Assert.False(longHashSet.Has(77777721321233L));
    }
}