using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public abstract class IntHashMapTests<T>(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    protected abstract T CreateValue();

    [Fact]
    public void Constructor_CreatesEmptyHashMap() {
        var hashMap = new IntHashMap<int>();

        Assert.Equal(0, hashMap.length);
        Assert.Equal(0, hashMap.lastIndex);
        Assert.Equal(-1, hashMap.freeIndex);
        Assert.NotNull(hashMap.buckets);
        Assert.NotNull(hashMap.slots);
        Assert.NotNull(hashMap.data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-100000)]
    [InlineData(10000000)]
    public void Constructor_CreatesHashMapWithCorrectCapacity(int initialCapacity) {
        var hashMap = new IntHashMap<int>(initialCapacity);

        Assert.True(hashMap.capacity >= initialCapacity);
        Assert.Equal(0, hashMap.length);
        Assert.Equal(0, hashMap.lastIndex);
        Assert.Equal(-1, hashMap.freeIndex);
        Assert.Equal(hashMap.buckets.Length, hashMap.capacity);
        Assert.Equal(hashMap.slots.Length, hashMap.capacity);
        Assert.Equal(hashMap.data.Length, hashMap.capacity);
    }

    [Fact]
    public void Constructor_CreatesExactCopyOfOriginalHashMap() {
        var originalHashMap = new IntHashMap<int>();
        originalHashMap.Add(1, 100, out _);
        originalHashMap.Add(2, 200, out _);

        var copiedHashMap = new IntHashMap<int>(originalHashMap);

        Assert.Equal(originalHashMap.length, copiedHashMap.length);
        Assert.Equal(originalHashMap.lastIndex, copiedHashMap.lastIndex);
        Assert.Equal(originalHashMap.freeIndex, copiedHashMap.freeIndex);
        Assert.Equal(originalHashMap.capacity, copiedHashMap.capacity);
        Assert.Equal(originalHashMap.capacityMinusOne, copiedHashMap.capacityMinusOne);

        Assert.True(copiedHashMap.TryGetValue(1, out int value1));
        Assert.Equal(100, value1);
        Assert.True(copiedHashMap.TryGetValue(2, out int value2));
        Assert.Equal(200, value2);

        copiedHashMap.Add(3, 300, out _);
        Assert.Equal(2, originalHashMap.length);
        Assert.Equal(3, copiedHashMap.length);
    }

    [Fact]
    public void Constructor_WithEmptyHashMapCreatesEmptyHashMap() {
        var originalHashMap = new IntHashMap<int>();
        var copiedHashMap = new IntHashMap<int>(originalHashMap);

        Assert.Equal(0, copiedHashMap.length);
        Assert.Equal(0, copiedHashMap.lastIndex);
        Assert.Equal(-1, copiedHashMap.freeIndex);
    }

    [Fact]
    public void Add_AddsElementCorrectly() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        var added = hashMap.Add(42, value, out int slotIndex);

        Assert.True(added);
        Assert.Equal(1, hashMap.length);
        Assert.Equal(42, hashMap.GetKeyByIndex(slotIndex));
        Assert.True(hashMap.TryGetValue(42, out T storedValue));
        Assert.Equal(value, storedValue);
    }

    [Fact]
    public void Add_PreventsAddingDuplicateKey() {
        var hashMap = new IntHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        var added1 = hashMap.Add(42, value1, out _);
        var added2 = hashMap.Add(42, value2, out _);
        var added3 = hashMap.Add(42, value2, out _);

        Assert.True(added1);
        Assert.False(added2);
        Assert.False(added3);
        Assert.Equal(1, hashMap.length);
    }

    [Fact]
    public void AddAndRetrieve_MinAndMaxKeys() {
        var hashMap = new IntHashMap<T>();
        var minValue = CreateValue();
        var maxValue = CreateValue();

        hashMap.Add(0, minValue, out _);
        Assert.True(hashMap.TryGetValue(0, out T storedMinValue));
        Assert.Equal(minValue, storedMinValue);

        hashMap.Add(int.MaxValue, maxValue, out _);
        Assert.True(hashMap.TryGetValue(int.MaxValue, out T storedMaxValue));
        Assert.Equal(maxValue, storedMaxValue);
    }

    [Fact]
    public void AddRemove_RetrieveMaxKeys() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        hashMap.Add(int.MaxValue, value, out _);
        Assert.True(hashMap.Has(int.MaxValue));
        Assert.True(hashMap.TryGetValue(int.MaxValue, out T retrievedValue));
        Assert.Equal(value, retrievedValue);

        Assert.True(hashMap.Remove(int.MaxValue, out T removedValue));
        Assert.Equal(value, removedValue);
        Assert.False(hashMap.Has(int.MaxValue));
    }

    [Fact]
    public void Set_AddsNewElementIfKeyNotExists() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        var added = hashMap.Set(42, value, out int slotIndex);

        Assert.True(added);
        Assert.Equal(1, hashMap.length);
        Assert.Equal(42, hashMap.GetKeyByIndex(slotIndex));
        Assert.True(hashMap.TryGetValue(42, out T storedValue));
        Assert.Equal(value, storedValue);
    }

    [Fact]
    public void Set_UpdatesValueIfKeyExists() {
        var hashMap = new IntHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        hashMap.Add(42, value1, out _);
        var updated = hashMap.Set(42, value2, out int slotIndex);

        Assert.False(updated);
        Assert.Equal(1, hashMap.length);
        Assert.Equal(42, hashMap.GetKeyByIndex(slotIndex));
        Assert.True(hashMap.TryGetValue(42, out T storedValue));
        Assert.Equal(value2, storedValue);
    }

    [Fact]
    public void Remove_RemovesExistingElement() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42, value, out _);
        var removed = hashMap.Remove(42, out T removedValue);

        Assert.True(removed);
        Assert.Equal(value, removedValue);
        Assert.Equal(0, hashMap.length);
        Assert.False(hashMap.Has(42));
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentKey() {
        var hashMap = new IntHashMap<T>();
        var removed = hashMap.Remove(42, out T removedValue);

        Assert.False(removed);
        Assert.Equal(default, removedValue);
        Assert.Equal(0, hashMap.length);
    }

    [Fact]
    public void Has_ReturnsTrueForExistingKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42, value, out _);

        Assert.True(hashMap.Has(42));
    }

    [Fact]
    public void Has_ReturnsFalseForNonExistentKey() {
        var hashMap = new IntHashMap<T>();
        Assert.False(hashMap.Has(42));
    }

    [Fact]
    public void TryGetValue_ReturnsValueForExistingKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42, value, out _);

        Assert.True(hashMap.TryGetValue(42, out T storedValue));
        Assert.Equal(value, storedValue);
    }

    [Fact]
    public void TryGetValue_ReturnsFalseForNonExistentKey() {
        var hashMap = new IntHashMap<T>();
        Assert.False(hashMap.TryGetValue(42, out T storedValue));
        Assert.Equal(default, storedValue);
    }

    [Fact]
    public void GetValueByKey_ReturnsValueForExistingKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42, value, out _);

        Assert.Equal(value, hashMap.GetValueByKey(42));
    }

    [Fact]
    public void GetValueByKey_ThrowsNonExistentKey() {
        var hashMap = new IntHashMap<T>();
        Assert.Throws<ArgumentException>(() => hashMap.GetValueByKey(42));
    }

    [Fact]
    public void TryGetValueRefByKey_ReturnsRefForExistingKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42, value, out _);
        ref T valueRef = ref hashMap.TryGetValueRefByKey(42, out bool exists);

        Assert.True(exists);
        Assert.Equal(value, valueRef);

        valueRef = CreateValue();
        Assert.True(hashMap.TryGetValue(42, out T updatedValue));
        Assert.Equal(valueRef, updatedValue);
    }

    [Fact]
    public void TryGetValueRefByKey_ReturnsFalseForNonExistentKey() {
        var hashMap = new IntHashMap<T>();
        ref T valueRef = ref hashMap.TryGetValueRefByKey(42, out bool exists);

        Assert.False(exists);
    }

    [Fact]
    public void GetValueRefByKey_ReturnsRefForExistingKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42, value, out _);
        ref T valueRef = ref hashMap.GetValueRefByKey(42);

        Assert.Equal(value, valueRef);

        valueRef = CreateValue();
        Assert.True(hashMap.TryGetValue(42, out T updatedValue));
        Assert.Equal(valueRef, updatedValue);
    }

    [Fact]
    public void GetValueRefByKey_CannotModifySourceCollectionByNotExistantKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42, value, out _);
        
        Assert.Throws<ArgumentException>(() => {
            ref T valueRef = ref hashMap.GetValueRefByKey(666);
            valueRef = default;
        });
        
        ref T valueRefThatExists = ref hashMap.GetValueRefByKey(42);
        Assert.Equal(value, valueRefThatExists);
    }

    [Fact]
    public void GetValueRefByIndex_ReturnsCorrectReference() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();
        hashMap.Add(42, value, out int slotIndex);
        ref T valueRef = ref hashMap.GetValueRefByIndex(slotIndex);

        Assert.Equal(value, valueRef);

        valueRef = CreateValue();
        Assert.Equal(valueRef, hashMap.GetValueByIndex(slotIndex));
    }

    [Fact]
    public void TryGetIndex_ReturnsCorrectIndexForExistingKey() {
        var hashMap = new IntHashMap<T>();
        var value = CreateValue();

        hashMap.Add(42, value, out int originalSlotIndex);

        int retrievedIndex = hashMap.TryGetIndex(42);

        Assert.Equal(originalSlotIndex, retrievedIndex);
    }

    [Fact]
    public void TryGetIndex_ReturnsMinusOneForNonExistentKey() {
        var hashMap = new IntHashMap<T>();
        int index = hashMap.TryGetIndex(42);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void Clear_ResetsHashMap() {
        var hashMap = new IntHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        hashMap.Add(42, value1, out _);
        hashMap.Add(43, value2, out _);

        hashMap.Clear();

        Assert.Equal(0, hashMap.length);
        Assert.False(hashMap.Has(42));
        Assert.False(hashMap.Has(43));
    }

    [Fact]
    public void CopyTo_CopiesAllElements() {
        var hashMap = new IntHashMap<T>();
        var values = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), };

        hashMap.Add(1, values[0], out _);
        hashMap.Add(2, values[1], out _);
        hashMap.Add(3, values[2], out _);
        hashMap.Add(4, values[3], out _);
        hashMap.Add(5, values[4], out _);
        hashMap.Add(6, values[5], out _);
        hashMap.Add(7, values[6], out _);
        hashMap.Add(8, values[7], out _);

        var destination = new T[8];
        hashMap.CopyTo(destination);

        Assert.Equal(values, destination);
    }

    [Fact]
    public void CopyTo_ThrowsExceptionIfDestinationTooSmall() {
        var hashMap = new IntHashMap<T>();
        hashMap.Add(1, CreateValue(), out _);
        hashMap.Add(2, CreateValue(), out _);

        var destination = new T[1];
        Assert.Throws<IndexOutOfRangeException>(() => hashMap.CopyTo(destination));
    }

    [Fact]
    public void Expand_HandlesLargeNumberOfElements() {
        var hashMap = new IntHashMap<T>();
        var values = new List<T>();

        for (int i = 0; i < 100000; i++) {
            var value = CreateValue();
            values.Add(value);
            hashMap.Add(i, value, out _);
        }

        Assert.Equal(100000, hashMap.length);

        for (int i = 0; i < 100000; i++) {
            Assert.True(hashMap.TryGetValue(i, out T storedValue));
            Assert.Equal(values[i], storedValue);
        }
    }

    [Fact]
    public void CopyFrom_CopiesAllElementsFromAnotherHashMap() {
        var sourceHashMap = new IntHashMap<T>();
        var value1 = CreateValue();
        var value2 = CreateValue();

        sourceHashMap.Add(42, value1, out _);
        sourceHashMap.Add(43, value2, out _);

        var destinationHashMap = new IntHashMap<T>();
        destinationHashMap.CopyFrom(sourceHashMap);

        Assert.Equal(sourceHashMap.length, destinationHashMap.length);
        Assert.True(destinationHashMap.TryGetValue(42, out T storedValue1));
        Assert.True(destinationHashMap.TryGetValue(43, out T storedValue2));
        Assert.Equal(value1, storedValue1);
        Assert.Equal(value2, storedValue2);
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var hashMap = new IntHashMap<T>();
        var values = new HashSet<T>();
        var keys = new HashSet<int>();

        for (int i = 0; i < 10; i++) {
            var value = CreateValue();
            values.Add(value);
            keys.Add(i);
            hashMap.Add(i, value, out _);
        }

        var enumeratedKeys = new HashSet<int>();
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
        var hashMap = new IntHashMap<int>();
        int count = 0;

        foreach (var idx in hashMap) {
            count++;
        }

        Assert.Equal(0, count);
    }

    [Fact]
    public void ForEach_IteratesOverAllElements() {
        var hashMap = new IntHashMap<T>();
        var expectedKeys = new HashSet<int> { 1, 5, 10, 128, 512 };
        var expectedValues = new HashSet<T> { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        int i = 0;
        foreach (var key in expectedKeys) {
            hashMap.Add(key, expectedValues.ElementAt(i++), out _);
        }

        var resultKeys = new HashSet<int>();
        var resultValues = new HashSet<T>();

        foreach (var idx in hashMap) {
            resultKeys.Add(hashMap.GetKeyByIndex(idx));
            resultValues.Add(hashMap.GetValueByIndex(idx));
        }

        Assert.True(expectedKeys.SetEquals(resultKeys));
        Assert.True(expectedValues.SetEquals(resultValues));
    }

    [Fact]
    public void ForEach_GetValueRefIteratesOverAllElements() {
        var hashMap = new IntHashMap<T>();
        var expectedKeys = new HashSet<int> { 1, 5, 10, 128, 512 };
        var expectedValues = new HashSet<T> { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        int i = 0;
        foreach (var key in expectedKeys) {
            hashMap.Add(key, expectedValues.ElementAt(i++), out _);
        }

        var resultKeys = new HashSet<int>();
        var resultValues = new HashSet<T>();

        foreach (var idx in hashMap) {
            resultKeys.Add(hashMap.GetKeyByIndex(idx));
            resultValues.Add(hashMap.GetValueRefByIndex(idx));
        }

        Assert.True(expectedKeys.SetEquals(resultKeys));
        Assert.True(expectedValues.SetEquals(resultValues));
    }

    [Fact]
    public void ForEach_AfterRemovalSkipsRemovedEntries() {
        var hashMap = new IntHashMap<T>();
        var initialKeys = new HashSet<int> { 1, 5, 10, 128, 512, int.MaxValue };
        var initialValues = new List<T> { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        int i = 0;
        foreach (var key in initialKeys) {
            hashMap.Add(key, initialValues[i++], out _);
        }

        hashMap.Remove(128, out _);
        hashMap.Remove(10, out _);

        var resultKeys = new HashSet<int>();
        var resultValues = new HashSet<T>();
        foreach (var idx in hashMap) {
            resultKeys.Add(hashMap.GetKeyByIndex(idx));
            resultValues.Add(hashMap.GetValueByIndex(idx));
        }

        var expectedKeys = new HashSet<int> { 1, 5, 512, int.MaxValue };
        var expectedValues = new HashSet<T> { initialValues[0], initialValues[1], initialValues[4], initialValues[5] };
        Assert.True(expectedKeys.SetEquals(resultKeys));
        Assert.True(expectedValues.SetEquals(resultValues));
    }

    [Fact]
    public void ForEach_GetValueRefAfterRemovalSkipsRemovedEntries() {
        var hashMap = new IntHashMap<T>();
        var initialKeys = new HashSet<int> { 1, 5, 10, 128, 512, int.MaxValue };
        var initialValues = new List<T> { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        int i = 0;
        foreach (var key in initialKeys) {
            hashMap.Add(key, initialValues[i++], out _);
        }

        hashMap.Remove(128, out _);
        hashMap.Remove(10, out _);

        var resultKeys = new HashSet<int>();
        var resultValues = new HashSet<T>();
        foreach (var idx in hashMap) {
            resultKeys.Add(hashMap.GetKeyByIndex(idx));
            resultValues.Add(hashMap.GetValueRefByIndex(idx));
        }

        var expectedKeys = new HashSet<int> { 1, 5, 512, int.MaxValue };
        var expectedValues = new HashSet<T> { initialValues[0], initialValues[1], initialValues[4], initialValues[5] };
        Assert.True(expectedKeys.SetEquals(resultKeys));
        Assert.True(expectedValues.SetEquals(resultValues));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(69)]
    [InlineData(665566)]
    public void Stress_AddRemoveOperations(int seed) {
        var hashMap = new IntHashMap<T>(8);
        var tracker = new Dictionary<int, T>();
        var random = new Random(seed);
        const int operationsCount = 100000;

        for (int i = 0; i < operationsCount; i++) {
            var key = random.Next(operationsCount / 10);
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

    [Theory]
    [InlineData(5, 100)]
    [InlineData(2, 1000)]
    [InlineData(32, 12)]
    [InlineData(1024, 64)]
    public void Stress_SetRemoveCycle(int cycles, int valuesCount) {
        var hashMap = new IntHashMap<T>();
        var tracker = new Dictionary<int, T>();

        for (int cycle = 0; cycle < cycles; cycle++) {
            for (int i = 0; i < valuesCount; i++) {
                var value = CreateValue();
                hashMap.Set(i, value, out _);
                tracker[i] = value;
            }

            foreach (var key in tracker.Keys.Take(valuesCount / 2).ToList()) {
                Assert.True(hashMap.Remove(key, out _));
                tracker.Remove(key);
            }

            Assert.Equal(tracker.Count, hashMap.length);
            foreach (var kvp in tracker) {
                Assert.True(hashMap.TryGetValue(kvp.Key, out T storedValue));
                Assert.Equal(kvp.Value, storedValue);
            }
        }
    }

    [Fact]
    public void SparseKeyRange_LargeKeySpacing() {
        var hashMap = new IntHashMap<T>();
        var sparseKeys = new[] { 1, 1000, 1000000, 1000000000 };
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

public class IntHashMapIntTests(ITestOutputHelper output) : IntHashMapTests<int>(output) {
    private int value = 0;

    protected override int CreateValue() {
        return this.value++;
    }
}

public class IntHashMapObjectTests(ITestOutputHelper output) : IntHashMapTests<object>(output) {
    protected override object CreateValue() {
        return new object();
    }
}

public class IntHashMapStringTests(ITestOutputHelper output) : IntHashMapTests<string>(output) {
    private long value = 0;

    protected override string CreateValue() {
        return this.value++.ToString();
    }
}