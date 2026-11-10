using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class IntSlotMapTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly() {
        var slotMap = new IntSlotMap(10);

        Assert.Equal(0, slotMap.length);
        Assert.Equal(-1, slotMap.freeIndex);
        Assert.Equal(0, slotMap.lastIndex);
        Assert.True(slotMap.IsEmpty());
    }

    [Fact]
    public void Has_ReturnsTrueForExistingKey() {
        var slotMap = new IntSlotMap(10);
        slotMap.TakeSlot(42, out _);

        Assert.True(slotMap.Has(42));
    }

    [Fact]
    public void Has_ReturnsFalseForNonExistentKey() {
        var slotMap = new IntSlotMap(10);
        Assert.False(slotMap.Has(42));
    }

    [Fact]
    public void Remove_RemovesExistingKey() {
        var slotMap = new IntSlotMap(10);
        var originalSlotIndex = slotMap.TakeSlot(42, out _);

        Assert.False(slotMap.IsEmpty());
        Assert.True(slotMap.Remove(42, out int slotIndex));
        Assert.Equal(originalSlotIndex, slotIndex);
        Assert.False(slotMap.Has(42));
        Assert.Equal(0, slotMap.length);
        Assert.True(slotMap.IsEmpty());
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentKey() {
        var slotMap = new IntSlotMap(10);
        Assert.False(slotMap.Remove(42, out _));
    }

    [Fact]
    public void TryGetIndex_ReturnsCorrectIndexForExistingKey() {
        var slotMap = new IntSlotMap(10);
        int originalSlotIndex = slotMap.TakeSlot(42, out _);

        Assert.True(slotMap.TryGetIndex(42, out int retrievedIndex));
        Assert.Equal(originalSlotIndex, retrievedIndex);
    }

    [Fact]
    public void TryGetIndex_ReturnsFalseForNonExistentKey() {
        var slotMap = new IntSlotMap(10);
        Assert.False(slotMap.TryGetIndex(42, out _));
    }

    [Fact]
    public void IsKeySet_ReturnsTrueForExistingKey() {
        var slotMap = new IntSlotMap(10);
        slotMap.TakeSlot(42, out _);

        Assert.True(slotMap.IsKeySet(42, out int slotIndex));
        Assert.NotEqual(-1, slotIndex);
    }

    [Fact]
    public void IsKeySet_ReturnsFalseForNonExistentKey() {
        var slotMap = new IntSlotMap(10);

        Assert.False(slotMap.IsKeySet(42, out int slotIndex));
        Assert.Equal(-1, slotIndex);
    }

    [Fact]
    public void TakeSlot_AddsNewKey() {
        var slotMap = new IntSlotMap(10);
        slotMap.TakeSlot(42, out bool resized);

        Assert.False(resized);
        Assert.Equal(1, slotMap.length);
        Assert.True(slotMap.Has(42));
    }

    [Fact]
    public void GetOrTakeSlot_AddsNewKey() {
        var slotMap = new IntSlotMap(10);
        var slotIndex = slotMap.GetOrTakeSlot(42, out var exists, out var resized);

        Assert.False(exists);
        Assert.False(resized);
        Assert.Equal(1, slotMap.length);
        Assert.True(slotMap.TryGetIndex(42, out var foundIndex));
        Assert.Equal(slotIndex, foundIndex);
    }

    [Fact]
    public void GetOrTakeSlot_ReturnsExistingSlot() {
        var slotMap = new IntSlotMap(10);
        var slotIndex = slotMap.TakeSlot(42, out _);
        var existingIndex = slotMap.GetOrTakeSlot(42, out var exists, out var resized);

        Assert.True(exists);
        Assert.False(resized);
        Assert.Equal(slotIndex, existingIndex);
        Assert.Equal(1, slotMap.length);
    }

    [Fact]
    public void GetOrTakeSlot_ReusesFreedSlot() {
        var slotMap = new IntSlotMap(10);
        slotMap.TakeSlot(1, out _);
        var freedIndex = slotMap.TakeSlot(2, out _);
        slotMap.Remove(2, out _);

        var slotIndex = slotMap.GetOrTakeSlot(100_000, out var exists, out _);

        Assert.False(exists);
        Assert.Equal(freedIndex, slotIndex);
        Assert.Equal(100_000, slotMap.GetKeyBySlotIndex(slotIndex));
        Assert.True(slotMap.Has(1));
        Assert.False(slotMap.Has(2));
    }

    [Fact]
    public void GetOrTakeSlot_ResizesWhenCapacityIsFull() {
        var slotMap = new IntSlotMap(1);
        var resized = false;
        for (int i = 0; i < 10000; i++) {
            slotMap.GetOrTakeSlot(i, out _, out var r);
            resized |= r;
        }

        Assert.True(resized);
        Assert.Equal(10000, slotMap.length);
        for (int i = 0; i < 10000; i++) {
            Assert.True(slotMap.Has(i));
        }
    }

    [Fact]
    public void TakeSlot_ResizesWhenCapacityIsFull() {
        var slotMap = new IntSlotMap(1);
        var resized = false;
        Assert.True(slotMap.capacity < 10000);
        for (int i = 0; i < 10000; i++) {
            slotMap.TakeSlot(i, out var r);
            resized |= r;
        }

        Assert.True(resized);
        Assert.Equal(10000, slotMap.length);
        Assert.True(slotMap.Has(3));
    }

    [Fact]
    public void Clear_ResetsMapToInitialState() {
        var slotMap = new IntSlotMap(10);

        slotMap.TakeSlot(42, out _);
        slotMap.Clear();

        Assert.Equal(0, slotMap.length);
        Assert.Equal(-1, slotMap.freeIndex);
        Assert.False(slotMap.Has(42));
    }

    [Fact]
    public void GetKeyBySlotIndex_ReturnsCorrectKey() {
        var slotMap = new IntSlotMap(10);
        int slotIndex = slotMap.TakeSlot(42, out _);

        Assert.Equal(42, slotMap.GetKeyBySlotIndex(slotIndex));
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var slotMap = new IntSlotMap(10);
        var keys = new int[] { 1, 5, 10, 15, 20 };

        foreach (var key in keys) {
            slotMap.TakeSlot(key, out _);
        }

        var enumeratedIndices = new List<int>();
        var enumerator = slotMap.GetEnumerator();

        while (enumerator.MoveNext()) {
            enumeratedIndices.Add(slotMap.GetKeyBySlotIndex(enumerator.Current));
        }

        Assert.Equal(keys.Length, slotMap.length);
        Assert.Equal(keys.OrderBy(k => k), enumeratedIndices.OrderBy(k => k));
    }

    [Fact]
    public void Dispose_ClearsAllData() {
        var slotMap = new IntSlotMap(10);

        slotMap.TakeSlot(42, out _);

        Assert.False(slotMap.IsEmpty());

        slotMap.Dispose();

        Assert.Equal(0, slotMap.length);
        Assert.Equal(-1, slotMap.freeIndex);
        Assert.Equal(0, slotMap.capacity);
        Assert.True(slotMap.IsEmpty());
    }

    [Theory]
    [InlineData(8)]
    [InlineData(49)]
    [InlineData(629)]
    [InlineData(111111)]
    public void Stress_AddRemoveOperations(int seed) {
        var slotMap = new IntSlotMap(8);
        var tracker = new HashSet<int>();
        var random = new Random(seed);
        const int operationsCount = 100000;

        for (int i = 0; i < operationsCount; i++) {
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
                    slotMap.Remove(key, out _);
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
}