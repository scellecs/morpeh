using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class SwappableLongSlotMapTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly() {
        var slotMap = new SwappableLongSlotMap(10);

        Assert.Equal(0, slotMap.length);
        Assert.True(slotMap.IsEmpty());
    }

    [Fact]
    public void Has_ReturnsTrueForExistingKey() {
        var slotMap = new SwappableLongSlotMap(10);
        slotMap.TakeSlot(42, out _);

        Assert.True(slotMap.Has(42));
    }
    
    [Fact]
    public void Has_ReturnsTrueForLargeKey() {
        var slotMap = new SwappableLongSlotMap(10);
        const long key = long.MaxValue - 420;
        slotMap.TakeSlot(key, out _);

        Assert.True(slotMap.Has(key));
    }

    [Fact]
    public void Has_ReturnsFalseForNonExistentKey() {
        var slotMap = new SwappableLongSlotMap(10);
        Assert.False(slotMap.Has(42));
    }

    [Fact]
    public void Remove_RemovesExistingKey() {
        var slotMap = new SwappableLongSlotMap(10);
        var originalSlotIndex = slotMap.TakeSlot(42, out _);

        Assert.False(slotMap.IsEmpty());
        Assert.True(slotMap.Remove(42, out int slotIndex, out int swappedFromSlotIndex));
        Assert.Equal(-1, swappedFromSlotIndex);
        Assert.Equal(originalSlotIndex, slotIndex);
        Assert.False(slotMap.Has(42));
        Assert.Equal(0, slotMap.length);
        Assert.True(slotMap.IsEmpty());
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentKey() {
        var slotMap = new SwappableLongSlotMap(10);
        Assert.False(slotMap.Remove(42, out var slotIndex, out var swappedFromSlotIndex));
        Assert.Equal(0, slotIndex);
        Assert.Equal(-1, swappedFromSlotIndex);
        Assert.Equal(0, slotMap.length);
    }
    
    [Fact]
    public void RemoveSwapBack_BehavesCorrectly() {
        var slotMap = new SwappableLongSlotMap(10);
        slotMap.TakeSlot(42, out _);
        var secondSlotIndex = slotMap.TakeSlot(43, out _);
        slotMap.TakeSlot(44, out _);
        slotMap.TakeSlot(45, out _);
        
        // Check initial data
        Assert.Equal(4, slotMap.length);

        // Removal of an element in the middle
        Assert.Equal(1, secondSlotIndex);
        Assert.True(slotMap.Remove(43, out var slotIndex, out var oldSlotIndex));
        Assert.Equal(slotIndex, secondSlotIndex);
        Assert.Equal(3, oldSlotIndex);
        Assert.Equal(45, slotMap.GetKeyBySlotIndex(secondSlotIndex));
        Assert.False(slotMap.Has(43));
        Assert.Equal(3, slotMap.length);
        
        // Removal of the right-most element
        Assert.True(slotMap.Remove(44, out slotIndex, out oldSlotIndex));
        Assert.Equal(2, slotIndex);
        Assert.Equal(-1, oldSlotIndex);
        Assert.False(slotMap.Has(44));
        Assert.Equal(2, slotMap.length);
        
        // Removal of the only element, but first remove the remaining elements except the first one
        Assert.True(slotMap.Remove(45, out slotIndex, out oldSlotIndex));
        Assert.Equal(1, slotIndex);
        Assert.Equal(-1, oldSlotIndex);
        Assert.False(slotMap.Has(45));
        Assert.Equal(1, slotMap.length);
        // Now remove the only element
        Assert.True(slotMap.Remove(42, out slotIndex, out oldSlotIndex));
        Assert.Equal(0, slotIndex);
        Assert.Equal(-1, oldSlotIndex);
        Assert.False(slotMap.Has(42));
        Assert.Equal(0, slotMap.length);
    }

    [Fact]
    public void TryGetIndex_ReturnsCorrectIndexForExistingKey() {
        var slotMap = new SwappableLongSlotMap(10);
        int originalSlotIndex = slotMap.TakeSlot(42, out _);

        Assert.True(slotMap.TryGetIndex(42, out int retrievedIndex));
        Assert.Equal(originalSlotIndex, retrievedIndex);
    }

    [Fact]
    public void TryGetIndex_ReturnsFalseForNonExistentKey() {
        var slotMap = new SwappableLongSlotMap(10);
        Assert.False(slotMap.TryGetIndex(42, out _));
    }

    [Fact]
    public void IsKeySet_ReturnsTrueForExistingKey() {
        var slotMap = new SwappableLongSlotMap(10);
        slotMap.TakeSlot(42, out _);

        Assert.True(slotMap.IsKeySet(42, out int slotIndex));
        Assert.NotEqual(-1, slotIndex);
    }

    [Fact]
    public void IsKeySet_ReturnsFalseForNonExistentKey() {
        var slotMap = new SwappableLongSlotMap(10);

        Assert.False(slotMap.IsKeySet(42, out int slotIndex));
        Assert.Equal(-1, slotIndex);
    }

    [Fact]
    public void TakeSlot_AddsNewKey() {
        var slotMap = new SwappableLongSlotMap(10);
        slotMap.TakeSlot(42, out bool resized);

        Assert.False(resized);
        Assert.Equal(1, slotMap.length);
        Assert.True(slotMap.Has(42));
    }

    [Fact]
    public void TakeSlot_ResizesWhenCapacityIsFull() {
        var slotMap = new SwappableLongSlotMap(1);
        var resized = false;
        Assert.True(slotMap.capacity < 10000);
        for (long i = 0; i < 10000; i++) {
            slotMap.TakeSlot(i, out var r);
            resized |= r;
        }

        Assert.True(resized);
        Assert.Equal(10000, slotMap.length);
        Assert.True(slotMap.Has(3));
    }

    [Fact]
    public void Clear_ResetsMapToInitialState() {
        var slotMap = new SwappableLongSlotMap(10);

        slotMap.TakeSlot(42, out _);
        slotMap.Clear();

        Assert.Equal(0, slotMap.length);
        Assert.False(slotMap.Has(42));
    }

    [Fact]
    public void GetKeyBySlotIndex_ReturnsCorrectKey() {
        var slotMap = new SwappableLongSlotMap(10);
        int slotIndex = slotMap.TakeSlot(42, out _);

        Assert.Equal(42, slotMap.GetKeyBySlotIndex(slotIndex));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(49)]
    [InlineData(629)]
    [InlineData(111111)]
    public void Stress_AddRemoveOperations(int seed) {
        var slotMap = new SwappableLongSlotMap(8);
        var tracker = new HashSet<long>();
        var random = new Random(seed);
        const int operationsCount = 100000;

        for (long i = 0; i < operationsCount; i++) {
            var key = random.Next(operationsCount / 10);
            var operation = random.NextSingle();

            if (operation < 0.7f) {
                var wasAdded = slotMap.IsKeySet(key, out _);
                if (wasAdded) {
                    slotMap.TakeSlot(key, out _);
                    tracker.Add(key);
                }
            }
            else {
                var wasRemoved = slotMap.Has(key);
                if (wasRemoved) {
                    slotMap.Remove(key, out _, out _);
                    tracker.Remove(key);
                }
            }

            Assert.Equal(tracker.Count, slotMap.length);

            if (i % 10000 == 0) {
                foreach (var item in tracker) {
                    Assert.True(slotMap.Has(item));
                }
            }
        }

        Assert.Equal(tracker.Count, slotMap.length);
        foreach (var item in tracker) {
            Assert.True(slotMap.Has(item));
        }
    }
    
    [Theory]
    [InlineData(8)]
    [InlineData(49)]
    [InlineData(629)]
    [InlineData(111111)]
    public void Stress_AllCollidableKeysChaotic(int seed) {
        var slotMap = new SwappableLongSlotMap(10);
        var random  = new Random(seed);
        
        var collisions = new long[50];
        for (var i = 0; i < collisions.Length; i++) {
            collisions[i] = i * 10;
        }
        
        for (int i = 0, length = collisions.Length; i < length; i++) {
            slotMap.TakeSlot(collisions[i], out _);
        }
        
        for (int i = 0, length = collisions.Length; i < length; i++) {
            Assert.True(slotMap.Has(collisions[i]));
        }
        
        var keysToRemove = collisions.OrderBy(_ => random.Next()).Take(collisions.Length / 2).ToArray();
        
        for (int i = 0, length = keysToRemove.Length; i < length; i++) {
            var key = keysToRemove[i];
            
            Assert.True(slotMap.Remove(key, out _, out _));
            Assert.False(slotMap.Has(key));
        }

        var remainder = collisions.Except(keysToRemove).ToArray();
        for (int i = 0, length = remainder.Length; i < length; i++) {
            Assert.True(slotMap.Has(remainder[i]));
        }
    }
}