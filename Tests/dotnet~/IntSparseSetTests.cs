using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class IntSparseSetTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Constructor_InitializesWithSpecifiedCapacity() {
        var set = new IntSparseSet(100);

        Assert.Equal(100, set.capacity);
        Assert.Equal(0, set.count);
        Assert.Equal(100, set.sparse.Length);
        Assert.Equal(100, set.dense.Length);
    }

    [Fact]
    public void Resize_IncreasesCapacity() {
        var set = new IntSparseSet(1);
        set.Resize(20);

        Assert.True(set.capacity >= 20);
        Assert.True(set.sparse.Length >= 20);
        Assert.True(set.dense.Length >= 20);
    }

    [Fact]
    public void Add_AddsElementCorrectly() {
        var set = new IntSparseSet(100);
        var result = set.Add(42);

        Assert.True(result);
        Assert.Equal(1, set.count);
        Assert.True(set.Contains(42));
    }

    [Fact]
    public void Add_DoesNotAddDuplicates() {
        var set = new IntSparseSet(100);
        set.Add(42);
        var result = set.Add(42);

        Assert.False(result);
        Assert.Equal(1, set.count);
    }

    [Fact]
    public void Add_HandlesMultipleElements() {
        var set = new IntSparseSet(100);
        set.Add(1);
        set.Add(2);
        set.Add(3);

        Assert.Equal(3, set.count);
        Assert.True(set.Contains(1));
        Assert.True(set.Contains(2));
        Assert.True(set.Contains(3));
    }

    [Fact]
    public void Contains_ReturnsFalseForNonExistentElement() {
        var set = new IntSparseSet(100);
        set.Add(42);

        Assert.False(set.Contains(24));
    }

    [Fact]
    public void Contains_ReturnsTrueForExistingElement() {
        var set = new IntSparseSet(100);
        set.Add(42);

        Assert.True(set.Contains(42));
    }

    [Fact]
    public void Contains_ReturnsFalseForOutOfRangeValue() {
        var set = new IntSparseSet(1);
        Assert.False(set.Contains(20));
    }

    [Fact]
    public void Remove_RemovesExistingElement() {
        var set = new IntSparseSet(100);
        set.Add(42);

        var result = set.Remove(42);

        Assert.True(result);
        Assert.Equal(0, set.count);
        Assert.False(set.Contains(42));
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistentElement() {
        var set = new IntSparseSet(100);
        set.Add(42);

        var result = set.Remove(24);

        Assert.False(result);
        Assert.Equal(1, set.count);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(100000)]
    public void Remove_HandlesMultipleOperations(int elementsCount) {
        var set = new IntSparseSet(elementsCount);
        var reference = new HashSet<int>();
        var random = new Random(42);

        for (int i = 0; i < elementsCount; i++) {
            var value = random.Next(elementsCount / 2);
            set.Add(value);
            reference.Add(value);
        }

        Assert.Equal(reference.Count, set.count);

        foreach (var item in reference) {
            Assert.True(set.Contains(item));
        }

        var elementsToRemove = reference.OrderBy(x => random.Next()).Take(reference.Count / 2).ToList();

        foreach (var item in elementsToRemove) {
            Assert.True(set.Remove(item));
            reference.Remove(item);
        }

        Assert.Equal(reference.Count, set.count);

        foreach (var item in reference) {
            Assert.True(set.Contains(item));
        }

        foreach (var item in elementsToRemove) {
            Assert.False(set.Contains(item));
        }
    }

    [Fact]
    public void Clear_RemovesAllElements() {
        var set = new IntSparseSet(100);
        set.Add(1);
        set.Add(2);
        set.Add(3);

        set.Clear();

        Assert.Equal(0, set.count);
        Assert.False(set.Contains(1));
        Assert.False(set.Contains(2));
        Assert.False(set.Contains(3));
    }

    [Fact]
    public void Clear_WorksOnEmptySet() {
        var set = new IntSparseSet(100);
        set.Clear();
        Assert.Equal(0, set.count);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Stress_AddRemoveOperations(int operationsCount) {
        var set = new IntSparseSet(operationsCount);
        var reference = new HashSet<int>();
        var random = new Random(42);

        for (int i = 0; i < operationsCount; i++) {
            var value = random.Next(operationsCount / 2);
            var operation = random.NextSingle();

            if (operation < 0.7f) {
                Assert.Equal(reference.Add(value), set.Add(value));
            }
            else {
                Assert.Equal(reference.Remove(value), set.Remove(value));
            }

            Assert.Equal(reference.Count, set.count);

            if (i % 1000 == 0) {
                foreach (var item in reference) {
                    Assert.True(set.Contains(item));
                }
            }
        }

        Assert.Equal(reference.Count, set.count);
        foreach (var item in reference) {
            Assert.True(set.Contains(item));
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Add_ThrowsOnNegativeValues(int value) {
        var set = new IntSparseSet(100);

        Assert.Throws<IndexOutOfRangeException>(() => set.Add(value));
        Assert.Equal(0, set.count);
    }


    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Contains_ThrowsOnNegativeValues(int value) {
        var set = new IntSparseSet(100);
        set.Add(69);

        Assert.Throws<IndexOutOfRangeException>(() => set.Contains(value));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Remove_ThrowsOnNegativeValues(int value) {
        var set = new IntSparseSet(100);
        set.Add(69);

        Assert.Throws<IndexOutOfRangeException>(() => set.Remove(value));
        Assert.Equal(1, set.count);
    }
}