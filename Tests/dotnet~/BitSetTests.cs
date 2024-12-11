using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class BitSetTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

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
    public void Set_Incrementally() {
        var bitSet = new BitSet();
        
        for (var i = 0; i < 2048; i++) {
            this.TestValue(bitSet, i);
        }
    }
    
    [Fact]
    public void Set_Decrementally() {
        var bitSet = new BitSet();
        
        for (var i = 2047; i >= 0; i--) {
            this.TestValue(bitSet, i);
        }
    }
    
    [Fact]
    public void Set_RandomlyDeterministically() {
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

    [Fact]
    public void Constructor_WithCapacity_CreatesCorrectSize() {
        var capacities = new[] { 64, 128, 256, 1024 };

        foreach (var capacity in capacities) {
            var bitSet = new BitSet(capacity);
            Assert.Equal(BitSet.GetMinLengthForCapacity(capacity), bitSet.longsCapacity);
        }
    }

    [Fact]
    public void Constructor_WithSetBitsInitializesCorrectly() {
        var setBits = new[] { 0, 63, 64, 127, 128, 1000 };
        var bitSet = new BitSet(setBits);

        foreach (var bit in setBits) {
            Assert.True(bitSet.IsSet(bit));
        }

        var unsetBits = new[] { 1, 62, 65, 126, 129, 999, 1001 };
        foreach (var bit in unsetBits) {
            Assert.False(bitSet.IsSet(bit));
        }
    }

    [Fact]
    public void Clear_ResetsAllBits() {
        var setBits = new[] { 0, 63, 64, 127, 128, 1000 };
        var bitSet = new BitSet(setBits);

        bitSet.Clear();

        foreach (var bit in setBits) {
            Assert.False(bitSet.IsSet(bit));
            Assert.Equal(0, bitSet.ValueOf(bit));
        }
    }

    [Fact]
    public void Set_ReturnsTrueOnlyWhenBitWasntSet() {
        var bitSet = new BitSet();

        Assert.True(bitSet.Set(42));
        Assert.False(bitSet.Set(42));
        bitSet.Unset(42);
        Assert.True(bitSet.Set(42));
    }

    [Fact]
    public void Unset_ReturnsTrueOnlyWhenBitWasSet() {
        var bitSet = new BitSet();

        Assert.False(bitSet.Unset(42));
        bitSet.Set(42);
        Assert.True(bitSet.Unset(42));
        Assert.False(bitSet.Unset(42));
    }

    [Fact]
    public void ExpandTo_IncreasesCapacity() {
        var bitSet = new BitSet(64);
        var initialCapacity = bitSet.longsCapacity;
        bitSet.Set(128);

        Assert.True(bitSet.longsCapacity > initialCapacity);
        Assert.True(bitSet.IsSet(128));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(1000)]
    public void SetAndUnset_PreservesOtherBits(int testBit) {
        var bitSet = new BitSet();
        var setBits = new[] { 0, 63, 64, 127, 128, 1000 };

        foreach (var bit in setBits) {
            bitSet.Set(bit);
        }

        bitSet.Set(testBit);
        bitSet.Unset(testBit);

        foreach (var bit in setBits) {
            if (bit != testBit) {
                Assert.True(bitSet.IsSet(bit));
            }
        }
    }

    [Fact]
    public void GetMinLengthForCapacity_ReturnsCorrectValues() {
        var testCases = new Dictionary<int, int> {
            { 1, 1 },
            { 63, 1 },
            { 64, 2 },
            { 65, 2 },
            { 128, 3 },
            { 1000, 16 }
        };

        foreach (var testCase in testCases) {
            Assert.Equal(testCase.Value, BitSet.GetMinLengthForCapacity(testCase.Key));
        }
    }
}