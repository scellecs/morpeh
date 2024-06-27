namespace Tests;

using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

[Collection("Sequential")]
public class BitSetTests {
    private readonly ITestOutputHelper output;
    
    public BitSetTests(ITestOutputHelper output) {
        this.output = output;
    }

    private void TestValue(BitSet bitSet, int value) {
        this.output.WriteLine($"Testing value {value}");
        
        Assert.False(bitSet.IsSet(value));
        Assert.Equal(0, bitSet.ValueOf(value));
        bitSet.Set(value);
        Assert.True(bitSet.IsSet(value));
        Assert.Equal(1, bitSet.ValueOf(value));
        bitSet.Unset(value);
        Assert.False(bitSet.IsSet(value));
        Assert.Equal(0, bitSet.ValueOf(value));
    }
    
    [Fact]
    public void SetIncrementally() {
        var bitSet = new BitSet();
        
        for (var i = 0; i < 2048; i++) {
            this.TestValue(bitSet, i);
        }
    }
    
    [Fact]
    public void SetDecrementally() {
        var bitSet = new BitSet();
        
        for (var i = 2047; i >= 0; i--) {
            this.TestValue(bitSet, i);
        }
    }
    
    [Fact]
    public void SetRandomlyDeterministically() {
        var bitSet = new BitSet();
        
        var seeds = new int[] {4, 20, 42, 69, 420, 690, 1337, 4269};
        
        foreach (var seed in seeds) {
            this.output.WriteLine($"Testing seed {seed}");
            
            var random = new Random(seed);
            
            for (var j = 0; j < 2048; j++) {
                var value = random.Next(0, 2048);
                this.TestValue(bitSet, value);
            }
        }
    }
}