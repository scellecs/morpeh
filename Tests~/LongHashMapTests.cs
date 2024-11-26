using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public abstract class LongHashMapTests<T>(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    protected abstract T CreateValue();

    [Fact]
    public void Constructor_CreatesEmptyHashMap()
    {
        var hashMap = new LongHashMap<int>();

        Assert.Equal(0, hashMap.length);
        Assert.Equal(0, hashMap.lastIndex);
        Assert.Equal(-1, hashMap.freeIndex);
        Assert.NotNull(hashMap.buckets);
        Assert.NotNull(hashMap.slots);
        Assert.NotNull(hashMap.data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    [InlineData(-99999)]
    [InlineData(66666)]
    public void Constructor_CreatesHashMapWithCorrectCapacity(int initialCapacity) {
        var hashMap = new LongHashMap<int>(initialCapacity);

        Assert.True(hashMap.capacity >= initialCapacity);
        Assert.Equal(0, hashMap.length);
        Assert.Equal(0, hashMap.lastIndex);
        Assert.Equal(-1, hashMap.freeIndex);
        Assert.Equal(hashMap.buckets.Length, hashMap.capacity);
        Assert.Equal(hashMap.slots.Length, hashMap.capacity);
        Assert.Equal(hashMap.data.Length, hashMap.capacity);
    }

    [Fact]
    public void Add_AddsElementCorrectly() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        var added = hashMap.Add(42L, value, out int slotIndex);

        Assert.True(added);
        Assert.Equal(1, hashMap.length);
        Assert.Equal(42L, hashMap.GetKeyByIndex(slotIndex));
        Assert.True(hashMap.TryGetValue(42L, out T storedValue));
        Assert.Equal(value, storedValue);
    }

    [Fact]
    public void Add_PreventsAddingDuplicateKey() {
        var hashMap = new LongHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        var added1 = hashMap.Add(42L, value1, out _);
        var added2 = hashMap.Add(42L, value2, out _);
        var added3 = hashMap.Add(42L, value2, out _);

        Assert.True(added1);
        Assert.False(added2);
        Assert.False(added3);
        Assert.Equal(1, hashMap.length);
    }

    [Fact]
    public void AddAndRetrieve_MinAndMaxKeys() {
        var hashMap = new LongHashMap<T>();
        var minValue = CreateValue();
        var maxValue = CreateValue();

        hashMap.Add(0L, minValue, out _);
        Assert.True(hashMap.TryGetValue(0L, out T storedMinValue));
        Assert.Equal(minValue, storedMinValue);

        hashMap.Add(long.MaxValue, maxValue, out _);
        Assert.True(hashMap.TryGetValue(long.MaxValue, out T storedMaxValue));
        Assert.Equal(maxValue, storedMaxValue);
    }

    [Fact]
    public void AddRemove_RetrieveMaxKeys() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        hashMap.Add(long.MaxValue, value, out _);
        Assert.True(hashMap.Has(long.MaxValue));
        Assert.True(hashMap.TryGetValue(long.MaxValue, out T retrievedValue));
        Assert.Equal(value, retrievedValue);

        Assert.True(hashMap.Remove(long.MaxValue, out T removedValue));
        Assert.Equal(value, removedValue);
        Assert.False(hashMap.Has(long.MaxValue));
    }

    [Fact]
    public void Set_AddsNewElementIfKeyNotExists() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        var added = hashMap.Set(42L, value, out int slotIndex);

        Assert.True(added);
        Assert.Equal(1, hashMap.length);
        Assert.Equal(42L, hashMap.GetKeyByIndex(slotIndex));
        Assert.True(hashMap.TryGetValue(42L, out T storedValue));
        Assert.Equal(value, storedValue);
    }

    [Fact]
    public void Set_UpdatesValueIfKeyExists() {
        var hashMap = new LongHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        hashMap.Add(42L, value1, out _);
        var updated = hashMap.Set(42L, value2, out int slotIndex);

        Assert.False(updated);
        Assert.Equal(1, hashMap.length);
        Assert.Equal(42L, hashMap.GetKeyByIndex(slotIndex));
        Assert.True(hashMap.TryGetValue(42L, out T storedValue));
        Assert.Equal(value2, storedValue);
    }

    [Fact]
    public void Remove_RemovesExistingElement() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42L, value, out _);
        var removed = hashMap.Remove(42L, out T removedValue);

        Assert.True(removed);
        Assert.Equal(value, removedValue);
        Assert.Equal(0, hashMap.length);
        Assert.False(hashMap.Has(42L));
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentKey() {
        var hashMap = new LongHashMap<T>();
        var removed = hashMap.Remove(42L, out T removedValue);

        Assert.False(removed);
        Assert.Equal(default, removedValue);
        Assert.Equal(0, hashMap.length);
    }

    [Fact]
    public void Has_ReturnsTrueForExistingKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42L, value, out _);

        Assert.True(hashMap.Has(42L));
    }

    [Fact]
    public void Has_ReturnsFalseForNonExistentKey() {
        var hashMap = new LongHashMap<T>();
        Assert.False(hashMap.Has(42L));
    }

    [Fact]
    public void TryGetValue_ReturnsValueForExistingKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42L, value, out _);

        Assert.True(hashMap.TryGetValue(42L, out T storedValue));
        Assert.Equal(value, storedValue);
    }

    [Fact]
    public void TryGetValue_ReturnsFalseForNonExistentKey() {
        var hashMap = new LongHashMap<T>();
        Assert.False(hashMap.TryGetValue(42L, out T storedValue));
        Assert.Equal(default, storedValue);
    }

    [Fact]
    public void GetValueByKey_ReturnsValueForExistingKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42L, value, out _);

        Assert.Equal(value, hashMap.GetValueByKey(42L));
    }

    [Fact]
    public void GetValueByKey_ThrowsNonExistentKey() {
        var hashMap = new LongHashMap<T>();
        Assert.Throws<ArgumentException>(() => hashMap.GetValueByKey(42L));
    }

    [Fact]
    public void TryGetValueRefByKey_ReturnsRefForExistingKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42L, value, out _);
        ref T valueRef = ref hashMap.TryGetValueRefByKey(42L, out bool exists);

        Assert.True(exists);
        Assert.Equal(value, valueRef);

        valueRef = CreateValue();
        Assert.True(hashMap.TryGetValue(42L, out T updatedValue));
        Assert.Equal(valueRef, updatedValue);
    }

    [Fact]
    public void TryGetValueRefByKey_ReturnsFalseForNonExistentKey() {
        var hashMap = new LongHashMap<T>();
        ref T valueRef = ref hashMap.TryGetValueRefByKey(42L, out bool exists);

        Assert.False(exists);
    }

    [Fact]
    public void GetValueRefByKey_ReturnsRefForExistingKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42L, value, out _);
        ref T valueRef = ref hashMap.GetValueRefByKey(42L);

        Assert.Equal(value, valueRef);

        valueRef = CreateValue();
        Assert.True(hashMap.TryGetValue(42L, out T updatedValue));
        Assert.Equal(valueRef, updatedValue);
    }

    [Fact]
    public void GetValueRefByKey_CannotModifySourceCollectionByNotExistantKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42L, value, out _);
        
        Assert.Throws<ArgumentException>(() => {
            ref T valueRef = ref hashMap.GetValueRefByKey(666);
            valueRef = default;
        });

        ref T valueRefThatExists = ref hashMap.GetValueRefByKey(42L);
        Assert.Equal(value, valueRefThatExists);
    }

    [Fact]
    public void GetValueRefByIndex_ReturnsCorrectReference() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42L, value, out int slotIndex);
        ref T valueRef = ref hashMap.GetValueRefByIndex(slotIndex);

        Assert.Equal(value, valueRef);

        valueRef = CreateValue();
        Assert.Equal(valueRef, hashMap.GetValueByIndex(slotIndex));
    }

    [Fact]
    public void TryGetIndex_ReturnsCorrectIndexForExistingKey() {
        var hashMap = new LongHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42L, value, out int originalSlotIndex);

        int retrievedIndex = hashMap.TryGetIndex(42L);

        Assert.Equal(originalSlotIndex, retrievedIndex);
    }

    [Fact]
    public void TryGetIndex_ReturnsMinusOneForNonExistentKey() {
        var hashMap = new LongHashMap<T>();
        int index = hashMap.TryGetIndex(42L);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void Clear_ResetsHashMap() {
        var hashMap = new LongHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        hashMap.Add(42L, value1, out _);
        hashMap.Add(43L, value2, out _);

        hashMap.Clear();

        Assert.Equal(0, hashMap.length);
        Assert.False(hashMap.Has(42L));
        Assert.False(hashMap.Has(43L));
    }

    [Fact]
    public void CopyTo_CopiesAllElements() {
        var hashMap = new LongHashMap<T>();
        var values = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), };

        hashMap.Add(1L, values[0], out _);
        hashMap.Add(2L, values[1], out _);
        hashMap.Add(3L, values[2], out _);
        hashMap.Add(4L, values[3], out _);
        hashMap.Add(5L, values[4], out _);
        hashMap.Add(6L, values[5], out _);
        hashMap.Add(7L, values[6], out _);
        hashMap.Add(8L, values[7], out _);

        var destination = new T[8];
        hashMap.CopyTo(destination);

        Assert.Equal(values, destination);
    }

    [Fact]
    public void CopyTo_ThrowsExceptionIfDestinationTooSmall() {
        var hashMap = new LongHashMap<T>();
        hashMap.Add(1L, CreateValue(), out _);
        hashMap.Add(2L, CreateValue(), out _);

        var destination = new T[1];
        Assert.Throws<IndexOutOfRangeException>(() => hashMap.CopyTo(destination));
    }

    [Fact]
    public void Expand_HandlesLargeNumberOfElements() {
        var hashMap = new LongHashMap<T>();
        var values = new List<T>();

        for (long i = 0; i < 100000; i++) {
            var value = CreateValue();
            values.Add(value);
            hashMap.Add(i, value, out _);
        }

        Assert.Equal(100000, hashMap.length);

        for (long i = 0; i < 100000; i++) {
            Assert.True(hashMap.TryGetValue(i, out T storedValue));
            Assert.Equal(values[(int)i], storedValue);
        }
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var hashMap = new LongHashMap<T>();
        var values = new HashSet<T>();
        var keys = new HashSet<long>();

        for (long i = 0; i < 10; i++) {
            var value = CreateValue();
            values.Add(value);
            keys.Add(i);
            hashMap.Add(i, value, out _);
        }

        var enumeratedKeys = new HashSet<long>();
        var enumeratedValues = new HashSet<T>();

        var enumerator = hashMap.GetEnumerator();
        while (enumerator.MoveNext()) {
            enumeratedKeys.Add(hashMap.GetKeyByIndex(enumerator.Current));
            enumeratedValues.Add(hashMap.GetValueByIndex(enumerator.Current));
        }

        Assert.Equal(10, hashMap.length);
        Assert.True(keys.SetEquals(enumeratedKeys));
        Assert.True(values.SetEquals(enumeratedValues));
    }

    [Fact]
    public void ForEach_EmptyHashMap() {
        var hashMap = new LongHashMap<int>();
        int count = 0;

        foreach (var idx in hashMap) {
            count++;
        }

        Assert.Equal(0, count);
    }

    [Fact]
    public void ForEach_IteratesOverAllElements() {
        var hashMap = new LongHashMap<T>();
        var expectedKeys = new HashSet<long> { 1, 5, 10, 128, 512 };
        var expectedValues = new HashSet<T> { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        int i = 0;
        foreach (var key in expectedKeys) {
            hashMap.Add(key, expectedValues.ElementAt(i++), out _);
        }

        var resultKeys = new HashSet<long>();
        var resultValues = new HashSet<T>();

        foreach (var idx in hashMap) {
            resultKeys.Add(hashMap.GetKeyByIndex(idx));
            resultValues.Add(hashMap.GetValueByIndex(idx));
        }

        Assert.True(expectedKeys.SetEquals(resultKeys));
        Assert.True(expectedValues.SetEquals(resultValues));
    }

    [Fact]
    public void ForEach_AfterRemovalSkipsRemovedEntries() {
        var hashMap = new LongHashMap<T>();
        var initialKeys = new HashSet<long> { 1, 5, 10, 128, 512, long.MaxValue };
        var initialValues = new List<T> { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        int i = 0;
        foreach (var key in initialKeys) {
            hashMap.Add(key, initialValues[i++], out _);
        }

        hashMap.Remove(128, out _);
        hashMap.Remove(10, out _);

        var resultKeys = new HashSet<long>();
        var resultValues = new HashSet<T>();
        foreach (var idx in hashMap) {
            resultKeys.Add(hashMap.GetKeyByIndex(idx));
            resultValues.Add(hashMap.GetValueByIndex(idx));
        }

        var expectedKeys = new HashSet<long> { 1, 5, 512, long.MaxValue };
        var expectedValues = new HashSet<T> { initialValues[0], initialValues[1], initialValues[4], initialValues[5] };
        Assert.True(expectedKeys.SetEquals(resultKeys));
        Assert.True(expectedValues.SetEquals(resultValues));
    }

    [Theory]
    [InlineData(12)]
    [InlineData(872)]
    [InlineData(987332)]
    [InlineData(9)]
    public void Stress_AddRemoveOperations(int seed) {
        var hashMap = new LongHashMap<T>(8);
        var tracker = new Dictionary<long, T>();
        var random = new Random(seed);
        const int operationsCount = 100000;

        for (int i = 0; i < operationsCount; i++) {
            var key = (long)random.Next(operationsCount / 10);
            var operation = random.NextSingle();

            if (operation < 0.5f) {
                var value = CreateValue();
                var wasAddedToHashMap = hashMap.Add(key, value, out _);
                var wasAddedToReference = !tracker.ContainsKey(key);
                if (wasAddedToReference) {
                    tracker[key] = value;
                }
                Assert.Equal(wasAddedToReference, wasAddedToHashMap);
            }
            else if (operation < 0.8f) {
                var wasRemovedFromHashMap = hashMap.Remove(key, out var removedValueFromHashMap);
                var wasRemovedFromReference = tracker.Remove(key, out var removedValueFromReference);
                Assert.Equal(wasRemovedFromReference, wasRemovedFromHashMap);
                if (wasRemovedFromHashMap) {
                    Assert.Equal(removedValueFromReference, removedValueFromHashMap);
                }
            }
            else {
                var hasInHashMap = hashMap.TryGetValue(key, out var valueFromHashMap);
                var hasInReference = tracker.TryGetValue(key, out var valueFromReference);
                Assert.Equal(hasInReference, hasInHashMap);
                if (hasInReference) {
                    Assert.Equal(valueFromReference, valueFromHashMap);
                }
            }

            Assert.Equal(tracker.Count, hashMap.length);

            if (i % 10000 == 0) {
                foreach (var kvp in tracker) {
                    Assert.True(hashMap.TryGetValue(kvp.Key, out var mapValue));
                    Assert.Equal(kvp.Value, mapValue);
                }
            }
        }

        Assert.Equal(tracker.Count, hashMap.length);
        foreach (var (key, value) in tracker) {
            Assert.True(hashMap.TryGetValue(key, out var mapValue));
            Assert.Equal(value, mapValue);
        }
    }

    [Fact]
    public void SparseKeyRange_LargeKeySpacing() {
        var hashMap = new LongHashMap<T>();
        var sparseKeys = new[] { 1L, 1000L, 1000000L, 1000000000L };
        var values = sparseKeys.Select(k => CreateValue()).ToArray();

        for (int i = 0; i < sparseKeys.Length; i++) {
            hashMap.Add(sparseKeys[i], values[i], out _);
        }

        Assert.Equal(sparseKeys.Length, hashMap.length);

        for (int i = 0; i < sparseKeys.Length; i++) {
            Assert.True(hashMap.TryGetValue(sparseKeys[i], out T storedValue));
            Assert.Equal(values[i], storedValue);
        }
    }
}

public class LongHashMapIntTests(ITestOutputHelper output) : LongHashMapTests<int>(output) {
    private int value = 0;

    protected override int CreateValue() {
        return this.value++;
    }
}

public class LongHashMapObjectTests(ITestOutputHelper output) : LongHashMapTests<object>(output) {
    protected override object CreateValue() {
        return new object();
    }
}

public class LongHashMapStringTests(ITestOutputHelper output) : LongHashMapTests<string>(output) {
    private long value = 0;

    protected override string CreateValue() {
        return this.value++.ToString();
    }
}