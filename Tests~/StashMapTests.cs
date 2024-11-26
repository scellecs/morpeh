using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class StashMapTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly() {
        var stashMap = new StashMap(10);

        Assert.Equal(0, stashMap.length);
        Assert.Equal(-1, stashMap.freeIndex);
        Assert.Equal(0, stashMap.lastIndex);
        Assert.True(stashMap.IsEmpty());
    }

    [Fact]
    public void Has_ReturnsTrueForExistingKey() {
        var stashMap = new StashMap(10);
        stashMap.TakeSlot(42, out _);

        Assert.True(stashMap.Has(42));
    }

    [Fact]
    public void Has_ReturnsFalseForNonExistentKey() {
        var stashMap = new StashMap(10);
        Assert.False(stashMap.Has(42));
    }

    [Fact]
    public void Remove_RemovesExistingKey() {
        var stashMap = new StashMap(10);
        var originalSlotIndex = stashMap.TakeSlot(42, out _);

        Assert.False(stashMap.IsEmpty());
        Assert.True(stashMap.Remove(42, out int slotIndex));
        Assert.Equal(originalSlotIndex, slotIndex);
        Assert.False(stashMap.Has(42));
        Assert.Equal(0, stashMap.length);
        Assert.True(stashMap.IsEmpty());
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentKey() {
        var stashMap = new StashMap(10);
        Assert.False(stashMap.Remove(42, out _));
    }

    [Fact]
    public void TryGetIndex_ReturnsCorrectIndexForExistingKey() {
        var stashMap = new StashMap(10);
        int originalSlotIndex = stashMap.TakeSlot(42, out _);

        Assert.True(stashMap.TryGetIndex(42, out int retrievedIndex));
        Assert.Equal(originalSlotIndex, retrievedIndex);
    }

    [Fact]
    public void TryGetIndex_ReturnsFalseForNonExistentKey() {
        var stashMap = new StashMap(10);
        Assert.False(stashMap.TryGetIndex(42, out _));
    }

    [Fact]
    public void IsKeySet_ReturnsTrueForExistingKey() {
        var stashMap = new StashMap(10);
        stashMap.TakeSlot(42, out _);

        Assert.True(stashMap.IsKeySet(42, out int slotIndex));
        Assert.NotEqual(-1, slotIndex);
    }

    [Fact]
    public void IsKeySet_ReturnsFalseForNonExistentKey() {
        var stashMap = new StashMap(10);

        Assert.False(stashMap.IsKeySet(42, out int slotIndex));
        Assert.Equal(-1, slotIndex);
    }

    [Fact]
    public void TakeSlot_AddsNewKey() {
        var stashMap = new StashMap(10);
        stashMap.TakeSlot(42, out bool resized);

        Assert.False(resized);
        Assert.Equal(1, stashMap.length);
        Assert.True(stashMap.Has(42));
    }

    [Fact]
    public void TakeSlot_ResizesWhenCapacityIsFull() {
        var stashMap = new StashMap(1);
        var resized = false;
        Assert.True(stashMap.capacity < 10000);
        for (int i = 0; i < 10000; i++) {
            stashMap.TakeSlot(i, out var r);
            resized |= r;
        }

        Assert.True(resized);
        Assert.Equal(10000, stashMap.length);
        Assert.True(stashMap.Has(3));
    }

    [Fact]
    public void Clear_ResetsMapToInitialState() {
        var stashMap = new StashMap(10);

        stashMap.TakeSlot(42, out _);
        stashMap.Clear();

        Assert.Equal(0, stashMap.length);
        Assert.Equal(-1, stashMap.freeIndex);
        Assert.False(stashMap.Has(42));
    }

    [Fact]
    public void GetKeyBySlotIndex_ReturnsCorrectKey() {
        var stashMap = new StashMap(10);
        int slotIndex = stashMap.TakeSlot(42, out _);

        Assert.Equal(42, stashMap.GetKeyBySlotIndex(slotIndex));
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var stashMap = new StashMap(10);
        var keys = new int[] { 1, 5, 10, 15, 20 };

        foreach (var key in keys) {
            stashMap.TakeSlot(key, out _);
        }

        var enumeratedIndices = new List<int>();
        var enumerator = stashMap.GetEnumerator();

        while (enumerator.MoveNext()) {
            enumeratedIndices.Add(stashMap.GetKeyBySlotIndex(enumerator.Current));
        }

        Assert.Equal(keys.Length, stashMap.length);
        Assert.Equal(keys.OrderBy(k => k), enumeratedIndices.OrderBy(k => k));
    }

    [Fact]
    public void Dispose_ClearsAllData() {
        var stashMap = new StashMap(10);

        stashMap.TakeSlot(42, out _);

        Assert.False(stashMap.IsEmpty());

        stashMap.Dispose();

        Assert.Equal(0, stashMap.length);
        Assert.Equal(-1, stashMap.freeIndex);
        Assert.Equal(0, stashMap.capacity);
        Assert.True(stashMap.IsEmpty());
    }

    [Theory]
    [InlineData(8)]
    [InlineData(49)]
    [InlineData(629)]
    [InlineData(111111)]
    public void Stress_AddRemoveOperations(int seed) {
        var stashMap = new StashMap(8);
        var tracker = new HashSet<int>();
        var random = new Random(seed);
        const int operationsCount = 100000;

        for (int i = 0; i < operationsCount; i++) {
            var key = random.Next(operationsCount / 10);
            var operation = random.NextSingle();

            if (operation < 0.7f) {
                var wasAdded = stashMap.IsKeySet(key, out _);
                if (wasAdded) {
                    stashMap.TakeSlot(key, out _);
                    tracker.Add(key);
                }
            }
            else {
                var wasRemoved = stashMap.Has(key);
                if (wasRemoved) {
                    stashMap.Remove(key, out _);
                    tracker.Remove(key);
                }
            }

            Assert.Equal(tracker.Count, stashMap.length);

            if (i % 10000 == 0) {
                foreach (var item in tracker) {
                    Assert.True(stashMap.Has(item));
                }
            }
        }

        Assert.Equal(tracker.Count, stashMap.length);
        foreach (var item in tracker) {
            Assert.True(stashMap.Has(item));
        }
    }
}