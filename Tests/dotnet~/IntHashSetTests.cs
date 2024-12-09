using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class IntHashSetTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Constructor_CreatesEmptyHashSet() {
        var hashSet = new IntHashSet();

        Assert.Equal(0, hashSet.length);
        Assert.Equal(0, hashSet.lastIndex);
        Assert.Equal(-1, hashSet.freeIndex);
        Assert.True(hashSet.capacity > 0);
        Assert.NotNull(hashSet.buckets);
        Assert.NotNull(hashSet.slots);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Constructor_CreatesHashSetWithCorrectCapacity(int initialCapacity) {
        var hashSet = new IntHashSet(initialCapacity);

        Assert.Equal(0, hashSet.length);
        Assert.Equal(0, hashSet.lastIndex);
        Assert.Equal(-1, hashSet.freeIndex);
        Assert.True(hashSet.capacity >= initialCapacity);
        Assert.Equal(hashSet.buckets.Length, hashSet.capacity);
        Assert.Equal(hashSet.slots.Length, hashSet.capacity * 2);
        Assert.Equal(hashSet.capacityMinusOne, hashSet.capacity - 1);
    }

    [Fact]
    public void Add_AddsElementCorrectly() {
        var set = new IntHashSet();
        var result = set.Add(42);

        Assert.True(result);
        Assert.Equal(1, set.length);
        Assert.True(set.Has(42));
    }

    [Fact]
    public void Add_DoesNotAddDuplicates() {
        var set = new IntHashSet();
        set.Add(42);
        var result = set.Add(42);

        Assert.False(result);
        Assert.Equal(1, set.length);
    }

    [Fact]
    public void Add_HandlesMultipleElements() {
        var set = new IntHashSet();
        set.Add(1);
        set.Add(2);
        set.Add(3);

        Assert.Equal(3, set.length);
        Assert.True(set.Has(1));
        Assert.True(set.Has(2));
        Assert.True(set.Has(3));
    }

    [Fact]
    public void Has_ReturnsFalseForNonExistentElement() {
        var set = new IntHashSet();
        set.Add(42);

        Assert.False(set.Has(24));
    }

    [Fact]
    public void Has_ReturnsTrueForExistingElement() {
        var set = new IntHashSet();
        set.Add(42);

        Assert.True(set.Has(42));
    }

    [Fact]
    public void Remove_RemovesExistingElement() {
        var set = new IntHashSet();
        set.Add(42);

        var result = set.Remove(42);

        Assert.True(result);
        Assert.Equal(0, set.length);
        Assert.False(set.Has(42));
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentElement() {
        var set = new IntHashSet();
        set.Add(42);

        var result = set.Remove(24);

        Assert.False(result);
        Assert.Equal(1, set.length);
    }

    [Theory]
    [InlineData(4, 1000)]
    [InlineData(16, 10000)]
    [InlineData(32, 100000)]
    public void Remove_HandlesCollisions(int initialCapacity, int elementsCount) {
        var set = new IntHashSet(initialCapacity);
        var reference = new HashSet<int>();
        var random = new Random(42);

        for (int i = 0; i < elementsCount; i++) {
            var value = random.Next(elementsCount * 2);
            set.Add(value);
            reference.Add(value);
        }

        Assert.Equal(reference.Count, set.length);

        foreach (var item in reference) {
            Assert.True(set.Has(item));
        }

        var elementsToRemove = reference.OrderBy(x => random.Next()).Take(reference.Count / 2).ToList();

        foreach (var item in elementsToRemove) {
            Assert.True(set.Remove(item));
            reference.Remove(item);
        }

        Assert.Equal(reference.Count, set.length);

        foreach (var item in reference) {
            Assert.True(set.Has(item));
        }

        foreach (var item in elementsToRemove) {
            Assert.False(set.Has(item));
        }
    }

    [Fact]
    public void Clear_RemovesAllElements() {
        var set = new IntHashSet();
        set.Add(1);
        set.Add(2);
        set.Add(3);

        set.Clear();

        Assert.Equal(0, set.length);
        Assert.False(set.Has(1));
        Assert.False(set.Has(2));
        Assert.False(set.Has(3));
    }

    [Fact]
    public void Clear_WorksOnEmptySet() {
        var set = new IntHashSet();
        set.Clear();
        Assert.Equal(0, set.length);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void CopyToArray_CopiesAllElements(int size) {
        var set = new IntHashSet();
        var expectedNumbers = new int[size];

        for (int i = 1; i <= size; i++)
        {
            set.Add(i);
            expectedNumbers[i - 1] = i;
        }

        var array = new int[size];
        set.CopyTo(array);
        Array.Sort(array);
        Assert.Equal(expectedNumbers, array);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void CopyToHashSet_CopiesAllElements(int size) {
        var source = new IntHashSet();

        for (int i = 0; i <= size; i++) {
            source.Add(i);
        }

        var destination = new IntHashSet();
        source.CopyTo(destination);

        Assert.Equal(source.length, destination.length);
        for (int i = 0; i <= size; i++) {
            Assert.True(destination.Has(i));
        }
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var set = new IntHashSet();
        set.Add(1);
        set.Add(2);
        set.Add(3);

        var found = new HashSet<int>();
        var enumerator = set.GetEnumerator();

        while (enumerator.MoveNext()) {
            found.Add(enumerator.Current);
        }

        Assert.Equal(3, found.Count);
        Assert.Contains(1, found);
        Assert.Contains(2, found);
        Assert.Contains(3, found);
    }

    [Fact]
    public void Expand_PreservesElements() {
        var set = new IntHashSet(2);
        for (int i = 0; i < 100; i++) {
            set.Add(i);
        }

        Assert.Equal(100, set.length);
        for (int i = 0; i < 100; i++)
        {
            Assert.True(set.Has(i));
        }
    }

    [Fact]
    public void Constructor_InitializesWithSpecifiedCapacity() {
        var set = new IntHashSet(100);
        Assert.True(set.capacity >= 100);
        Assert.Equal(0, set.length);
    }

    [Fact]
    public void ForEach_IteratesOverAllElements() {
        var set = new IntHashSet();
        set.Add(1);
        set.Add(2);
        set.Add(3);

        var found = new HashSet<int>();
        foreach (var item in set) {
            found.Add(item);
        }

        Assert.Equal(3, found.Count);
        Assert.Contains(1, found);
        Assert.Contains(2, found);
        Assert.Contains(3, found);
    }

    [Fact]
    public void Operations_WithLargeSequentialDataset() {
        var set = new IntHashSet(16);
        var reference = new HashSet<int>();
        const int count = 100000;

        for (int i = 0; i < count; i++) {
            Assert.True(set.Add(i));
            reference.Add(i);
        }

        Assert.Equal(reference.Count, set.length);

        for (int i = 0; i < count; i++) {
            Assert.False(set.Add(i));
        }

        Assert.Equal(reference.Count, set.length);

        for (int i = 0; i < count; i += 2) {
            Assert.True(set.Remove(i));
            reference.Remove(i);
        }

        Assert.Equal(reference.Count, set.length);

        for (int i = 0; i < count; i++) {
            Assert.Equal(reference.Contains(i), set.Has(i));
        }
    }

    [Fact]
    public void Stress_AddRemoveOperations() {
        var set = new IntHashSet(8);
        var reference = new HashSet<int>();
        var random = new Random(42);
        const int operationsCount = 100000;

        for (int i = 0; i < operationsCount; i++) {
            var value = random.Next(operationsCount / 10);
            var operation = random.NextSingle();

            if (operation < 0.7f) {
                Assert.Equal(reference.Add(value), set.Add(value));
            }
            else {
                Assert.Equal(reference.Remove(value), set.Remove(value));
            }

            Assert.Equal(reference.Count, set.length);

            if (i % 10000 == 0) {
                foreach (var item in reference) {
                    Assert.True(set.Has(item));
                }
            }
        }

        Assert.Equal(reference.Count, set.length);
        foreach (var item in reference) {
            Assert.True(set.Has(item));
        }
    }
#pragma warning disable xUnit1013
    //[Fact] Broken/NotSupported
    public void ForEach_HandlesNegativeValues() {
        var set = new IntHashSet();
        set.Add(-1);
        set.Add(-42);
        set.Add(-100);
        set.Add(0);
        set.Add(1);

        var found = new HashSet<int>();
        foreach (var item in set) {
            found.Add(item);
        }

        Assert.Equal(5, found.Count);
        Assert.Contains(-1, found);
        Assert.Contains(-42, found);
        Assert.Contains(-100, found);
        Assert.Contains(0, found);
        Assert.Contains(1, found);
    }

    //[Fact] Broken/NotSupported
    public void Enumerator_HandlesNegativeValues() {
        var set = new IntHashSet();
        set.Add(-1);
        set.Add(-999);
        set.Add(-5000);
        set.Add(0);

        var found = new HashSet<int>();
        var enumerator = set.GetEnumerator();

        while (enumerator.MoveNext()) {
            found.Add(enumerator.Current);
        }

        Assert.Equal(4, found.Count);
        Assert.Contains(-1, found);
        Assert.Contains(-999, found);
        Assert.Contains(-5000, found);
        Assert.Contains(0, found);
    }

    //[Theory] Broken/NotSupported
    //[InlineData(new int[] { -1, -2, -3, -4, -5 })]
    //[InlineData(new int[] { -100, -200, -300, -400, -500 })]
    //[InlineData(new int[] { -1000, -2000, -3000, 0, 1000 })]
    public void CopyToArray_HandlesNegativeValues(int[] values) {
        var set = new IntHashSet();
        foreach (var value in values) {
            set.Add(value);
        }

        var array = new int[values.Length];
        set.CopyTo(array);
        Array.Sort(array);
        Array.Sort(values);

        Assert.Equal(values.Length, array.Length);
        Assert.Equal(values, array);
    }

    //[Fact] Broken/NotSupported
    public void CopyToHashSet_HandlesNegativeValues() {
        var source = new IntHashSet();
        var negativeValues = new[] { -1, -42, -100, -999, -5000, 0 };

        foreach (var value in negativeValues) {
            source.Add(value);
        }

        var destination = new IntHashSet();
        source.CopyTo(destination);

        Assert.Equal(source.length, destination.length);
        foreach (var value in negativeValues) {
            Assert.True(destination.Has(value));
        }
    }
#pragma warning restore xUnit1013
}