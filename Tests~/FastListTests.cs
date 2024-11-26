using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public abstract class FastListTests<T>(ITestOutputHelper output) {

    private readonly ITestOutputHelper output = output;

    protected abstract T CreateValue();
    
    protected virtual EqualityComparer<T> GetEqualityComparer() => EqualityComparer<T>.Default;

    [Fact]
    public void Constructor_CreatesEmptyList() {
        var list = new FastList<T>();

        Assert.Equal(0, list.length);
        Assert.True(list.capacity > 0);
        Assert.NotNull(list.data);
        Assert.Equal(EqualityComparer<T>.Default, list.comparer);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Constructor_CreatesListWithCorrectCapacity(int initialCapacity) {
        var list = new FastList<T>(initialCapacity);

        Assert.Equal(0, list.length);
        Assert.True(list.capacity >= initialCapacity);
        Assert.NotNull(list.data);
        Assert.Equal(list.data.Length, list.capacity);
        Assert.Equal(EqualityComparer<T>.Default, list.comparer);
    }

    [Fact]
    public void Constructor_CreatesExactCopyOfOriginalList() {
        var originalList = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();

        originalList.Add(value1);
        originalList.Add(value2);

        var copiedList = new FastList<T>(originalList);

        Assert.Equal(originalList.length, copiedList.length);
        Assert.Equal(originalList.capacity, copiedList.capacity);
        Assert.Equal(originalList.comparer, copiedList.comparer);

        Assert.Equal(value1, copiedList[0]);
        Assert.Equal(value2, copiedList[1]);

        copiedList.Add(CreateValue());
        Assert.Equal(2, originalList.length);
        Assert.Equal(3, copiedList.length);
    }

    [Fact]
    public void Constructor_CopyConstructorPreservesOriginalDataIntegrity() {
        var originalList = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();

        originalList.Add(value1);
        originalList.Add(value2);

        var copiedList = new FastList<T>(originalList);

        originalList.Add(CreateValue());

        Assert.Equal(2, copiedList.length);
        Assert.Equal(value1, copiedList[0]);
        Assert.Equal(value2, copiedList[1]);
    }

    [Fact]
    public void Add_AddsElementCorrectly() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value = CreateValue();
        var index = list.Add(value);

        Assert.Equal(0, index);
        Assert.Equal(1, list.length);
        Assert.Equal(value, list[0]);
    }

    [Fact]
    public void ToArray_CreatesCorrectArray() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();

        list.Add(value1);
        list.Add(value2);

        var array = list.ToArray();

        Assert.Equal(2, array.Length);
        Assert.Equal(new[] { value1, value2 }, array);
    }

    [Fact]
    public void ToArray_EmptyListReturnsEmptyArray() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var array = list.ToArray();

        Assert.Same(Array.Empty<T>(), array);
    }

    [Fact]
    public void CopyTo_CopiesAllElementsCorrectly() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };

        foreach (var value in values) {
            list.Add(value);
        }

        var destination = new T[list.length];
        list.CopyTo(destination);

        Assert.Equal(values, destination);
    }

    [Fact]
    public void AddRange_AddsMultipleElements() {
        var list1 = new FastList<T> { comparer = GetEqualityComparer() };
        var list2 = new FastList<T> { comparer = GetEqualityComparer() };

        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list1.Add(value1);
        list2.Add(value2);
        list2.Add(value3);

        list1.AddRange(list2);

        Assert.Equal(3, list1.length);
        Assert.Equal(new[] { value1, value2, value3 }, list1.ToArray());
    }

    [Fact]
    public void AddRange_DoesNotModifyListWithEmptySource() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        var originalLength = list.length;
        var originalCapacity = list.capacity;
        var originalValue = list[0];

        var emptyList = new FastList<T>();
        list.AddRange(emptyList);

        Assert.Equal(originalLength, list.length);
        Assert.Equal(originalCapacity, list.capacity);
        Assert.Equal(originalValue, list[0]);
    }

    [Fact]
    public void IndexOf_FindsCorrectIndex() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();

        list.Add(value1);
        list.Add(value2);

        Assert.Equal(1, list.IndexOf(value2));
        Assert.Equal(-1, list.IndexOf(CreateValue()));
    }

    [Fact]
    public void RemoveAt_ThrowsOnInvalidIndex() {
        var list = new FastList<int>();
        list.Add(42);

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
    }

    [Fact]
    public void RemoveAt_RemovesElementAtIndex() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        list.RemoveAt(1);

        Assert.Equal(2, list.length);
        Assert.Equal(new[] { value1, value3 }, list.ToArray());
    }

    [Fact]
    public void RemoveAt_RemovesElementFromBeginningOfList() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        list.RemoveAt(0);

        Assert.Equal(2, list.length);
        Assert.Equal(new[] { value2, value3 }, list.ToArray());
    }

    [Fact]
    public void RemoveAt_RemovesLastElement() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();

        list.Add(value1);
        list.Add(value2);

        list.RemoveAt(1);

        Assert.Equal(1, list.length);
        Assert.Equal(new[] { value1 }, list.ToArray());
    }

    [Fact]
    public void RemoveAtFast_RemovesElementAtIndex() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        list.RemoveAtFast(1);

        Assert.Equal(2, list.length);
        Assert.Equal(new[] { value1, value3 }, list.ToArray());
    }

    [Fact]
    public void Remove_RemovesElement() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        var removed = list.Remove(value2);

        Assert.True(removed);
        Assert.Equal(2, list.length);
        Assert.Equal(new[] { value1, value3 }, list.ToArray());
    }


    [Fact]
    public void Remove_RemovesFirstOccurrenceOfElement() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value1);

        var removed = list.Remove(value1);

        Assert.True(removed);
        Assert.Equal(2, list.length);
        Assert.Equal(new[] { value2, value1 }, list.ToArray());
    }

    [Fact]
    public void Remove_ReturnsFalseWhenElementNotFound() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());

        var removed = list.Remove(CreateValue());

        Assert.False(removed);
        Assert.Equal(2, list.length);
    }

    [Fact]
    public void RemoveSwapBack_RemovesElementAndSwapsWithLast() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        var removed = list.RemoveSwapBack(value2);

        Assert.True(removed);
        Assert.Equal(2, list.length);
        Assert.Equal(value3, list[1]);
    }

    [Fact]
    public void RemoveSwapBack_ReturnsFalseIfElementNotFound() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());

        var removed = list.RemoveSwapBack(CreateValue());

        Assert.False(removed);
        Assert.Equal(2, list.length);
    }

    [Fact]
    public void RemoveAtSwapBack_ThrowsOnInvalidIndex() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAtSwapBack(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAtSwapBack(1));
    }

    [Fact]
    public void RemoveAtSwapBack_SwapsWithLastElement() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        list.RemoveAtSwapBack(1);

        Assert.Equal(2, list.length);
        Assert.Equal(value1, list[0]);
        Assert.Equal(value3, list[1]);
    }

    [Fact]
    public void RemoveAtSwapBackFast_SwapsWithLastElement() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        list.RemoveAtSwapBackFast(1);

        Assert.Equal(2, list.length);
        Assert.Equal(value1, list[0]);
        Assert.Equal(value3, list[1]);
    }

    [Fact]
    public void RemoveRange_ThrowsOnInvalidIndex() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(2, 1));
    }

    [Fact]
    public void RemoveRange_ThrowsWhenCountExceedsBounds() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());

        Assert.Throws<ArgumentException>(() => list.RemoveRange(1, 2));
    }

    [Fact]
    public void RemoveRange_ThrowsOnNegativeCount() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(0, -1));
    }

    [Fact]
    public void RemoveRange_RemovesSpecifiedRange() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) list.Add(value);

        list.RemoveRange(1, 2);

        Assert.Equal(3, list.length);
        Assert.Equal(new[] { values[0], values[3], values[4] }, list.ToArray());
    }

    [Fact]
    public void RemoveRange_RemovesEntireRangeAtEnd() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) list.Add(value);

        list.RemoveRange(3, 2);

        Assert.Equal(3, list.length);
        Assert.Equal(new[] { values[0], values[1], values[2] }, list.ToArray());
    }

    [Fact]
    public void RemoveRange_ZeroCountDoesNotModifyList() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        var originalLength = list.length;
        var originalValue = list[0];

        list.RemoveRange(0, 0);

        Assert.Equal(originalLength, list.length);
        Assert.Equal(originalValue, list[0]);
    }

    [Fact]
    public void SwapFast_SwapsElements() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value1 = CreateValue();
        var value2 = CreateValue();
        var value3 = CreateValue();

        list.Add(value1);
        list.Add(value2);
        list.Add(value3);

        list.SwapFast(0, 2);

        Assert.Equal(3, list.length);
        Assert.Equal(value3, list[0]);
        Assert.Equal(value2, list[1]);
        Assert.Equal(value1, list[2]);
    }

    [Fact]
    public void SwapFast_SameIndexDoesNotModifyList() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value = CreateValue();
        var value2 = CreateValue();
        list.Add(value);
        list.Add(value2);

        list.SwapFast(0, 0);
        list.SwapFast(1, 1);

        Assert.Equal(value, list[0]);
        Assert.Equal(value2, list[1]);
    }

    [Fact]
    public void Clear_ResetsList() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());

        list.Clear();

        Assert.Equal(0, list.length);
    }

    [Fact]
    public void Clear_EmptyListDoesNothing() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var originalCapacity = list.capacity;

        list.Clear();

        Assert.Equal(0, list.length);
        Assert.Equal(originalCapacity, list.capacity);
    }

    [Fact]
    public void Indexer_SetValueAtValidIndex() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var originalValue = CreateValue();
        var newValue = CreateValue();

        list.Add(originalValue);
        Assert.Equal(originalValue, list[0]);
        list[0] = newValue;

        Assert.Equal(newValue, list[0]);
        Assert.NotEqual(originalValue, list[0]);
        Assert.Equal(1, list.length);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };

        Assert.Throws<ArgumentOutOfRangeException>(() => list[0] = CreateValue());
        Assert.Throws<ArgumentOutOfRangeException>(() => list[-1] = CreateValue());
    }

    [Fact]
    public void Indexer_SetMultipleValues() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };

        list.Add(values[0]);
        list.Add(values[1]);
        list.Add(values[2]);

        list[1] = CreateValue();

        Assert.Equal(3, list.length);
        Assert.NotEqual(values[1], list[1]);
    }

    [Fact]
    public void Grow_IncreasesCapacity() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());

        var originalLength = list.length;
        var originalValues = list.ToArray();

        list.Grow(10);

        Assert.True(list.capacity >= 10);
        Assert.Equal(originalLength, list.length);
        Assert.Equal(originalValues, list.ToArray());
    }

    [Fact]
    public void Grow_SmallerCapacityDoesNotShrink() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var originalCapacity = list.capacity;

        list.Grow(originalCapacity - 1);

        Assert.Equal(originalCapacity, list.capacity);
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) {
            list.Add(value);
        }

        int count = 0;
        var enumerator = list.GetEnumerator();
        while (enumerator.MoveNext()) {
            Assert.Equal(values[count], enumerator.Current);
            count++;
        }

        Assert.Equal(values.Length, count);
    }

    [Fact]
    public void ForEach_IteratesOverAllElements() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };

        foreach (var value in values) {
            list.Add(value);
        }

        var iteratedValues = new List<T>();
        foreach (var value in list) {
            iteratedValues.Add(value);
        }

        Assert.Equal(values, iteratedValues);
    }

    [Fact]
    public void ForEach_IteratesOverEmptyList() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };

        var iteratedValues = new List<T>();
        foreach (var value in list) {
            iteratedValues.Add(value);
        }

        Assert.Empty(iteratedValues);
    }

    [Fact]
    public void For_IteratesOverAllElements() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };

        foreach (var value in values) {
            list.Add(value);
        }

        var iteratedValues = new List<T>();
        for (int i = 0; i < list.length; i++) {
            iteratedValues.Add(list[i]);
        }

        Assert.Equal(values, iteratedValues);
    }

    [Fact]
    public void For_IterationWithRemoval() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new List<T>();

        for (int i = 0; i < 50; i++) {
            values.Add(CreateValue());
        }

        foreach (var value in values) {
            list.Add(value);
        }

        var length = list.length;
        var writeIndex = 0;

        for (int readIndex = 0; readIndex < length; readIndex++) {
            var element = list[readIndex];
            if (readIndex % 3 != 2) {
                list[writeIndex] = element;
                values[writeIndex] = element;
                writeIndex++;
            }
        }

        if (writeIndex != length) {
            list.RemoveRange(writeIndex, length - writeIndex);
            values.RemoveRange(writeIndex, length - writeIndex);
        }

        Assert.Equal(values.Count, list.length);

        for (int i = 0; i < list.length; i++) {
            Assert.Equal(values[i], list[i]);
        }
    }

    [Fact]
    public void For_IterationWithRemovalReverseLoop() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new List<T>();

        for (int i = 0; i < 50; i++) {
            values.Add(CreateValue());
        }

        foreach (var value in values) {
            list.Add(value);
        }

        for (int i = list.length - 1; i >= 0; i--) {
            if (i % 3 == 2) {
                list.RemoveAt(i);
                values.RemoveAt(i);
            }
        }

        Assert.Equal(values.Count, list.length);

        for (int i = 0; i < list.length; i++) {
            Assert.Equal(values[i], list[i]);
        }
    }

    [Fact]
    public void Sort_EmptyListDoesNotThrow() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        Assert.Empty(list.ToArray());
        list.Sort();
        Assert.Empty(list.ToArray());
    }

    [Fact]
    public void Sort_SingleElementRemainsUnchanged() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var value = CreateValue();
        list.Add(value);

        list.Sort();

        Assert.Single(list.ToArray());
        Assert.Equal(value, list[0]);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(5, 1)]
    public void Sort_RangeInvalidIndexThrowsArgumentOutOfRangeException(int index, int count) {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Sort(index, count));
    }

    [Theory]
    [InlineData(0, 6)]
    [InlineData(3, 3)]
    public void Sort_RangeInvalidCountThrowsArgumentException(int index, int count) {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());

        Assert.Throws<ArgumentException>(() => list.Sort(index, count));
    }

    [Fact]
    public void Sort_RangeNegativeCountThrowsArgumentOutOfRangeException() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(CreateValue());

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Sort(-1, 4));
    }

    [Fact]
    public void Sort_RangeZeroCountDoesNotModifyList() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) {
            list.Add(value);
        }

        var originalArray = list.ToArray();
        list.Sort(1, 0);

        Assert.Equal(originalArray, list.ToArray());
    }

    [Fact]
    public void SortDefaultComparer_MultipleElementsSortsCorrectly() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) {
            list.Add(value);
        }

        if (!values.All(x => x is IComparable)) {
            Assert.Throws<InvalidOperationException>(() => list.Sort());
            return;
        }

        var expectedArray = values.OrderBy(x => x).ToArray();
        list.Sort();

        Assert.Equal(expectedArray, list.ToArray());
    }

    [Fact]
    public void SortDefaultComparer_RangeSortsSpecifiedRangeOnly() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) {
            list.Add(value);
        }

        if (!values.All(x => x is IComparable)) {
            Assert.Throws<InvalidOperationException>(() => list.Sort());
            return;
        }

        var partialSort = values.ToArray();
        Array.Sort(partialSort, 1, 3);

        list.Sort(1, 3);

        Assert.Equal(partialSort, list.ToArray());
    }

    [Fact]
    public void SortDefaultComparer_PreservesElementsOutsideRange() {
        var list = new FastList<T> { comparer = GetEqualityComparer() };
        var values = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue(), CreateValue() };
        foreach (var value in values) {
            list.Add(value);
        }

        if (!values.All(x => x is IComparable)) {
            Assert.Throws<InvalidOperationException>(() => list.Sort());
            return;
        }

        var originalFirst = list[0];
        var originalLast = list[4];

        list.Sort(1, 3);

        Assert.Equal(originalFirst, list[0]);
        Assert.Equal(originalLast, list[4]);
    }
}

public class FastListIntTests(ITestOutputHelper output) : FastListTests<int>(output) {
    private int value = 0;

    protected override int CreateValue() {
        return this.value++;
    }
}

public class FastListObjectTests(ITestOutputHelper output) : FastListTests<object>(output) {
    protected override object CreateValue() {
        return new object();
    }
}

public class FastListStringTests(ITestOutputHelper output) : FastListTests<string>(output) {
    private long value = 0;

    protected override string CreateValue() {
        return this.value++.ToString();
    }
}

public class FastListCustomValueTypeTests(ITestOutputHelper output) : FastListTests<Point2D>(output) {
    private int nextCoord = 0;

    protected override Point2D CreateValue() {
        var coord = nextCoord++;
        return new Point2D(coord, coord * 2);
    }
}

public class FastListCustomReferenceTypeTests(ITestOutputHelper output) : FastListTests<Person>(output) {
    private int nextId = 0;

    protected override Person CreateValue() {
        var id = nextId++;
        return new Person($"Person{id}", id);
    }

    protected override EqualityComparer<Person> GetEqualityComparer() {
        return EqualityComparer<Person>.Default;
    }
}

public class FastListPrimitiveValueTypeTests(ITestOutputHelper output) : FastListTests<SimpleColor>(output) {
    private int r = 0;
    private int g = 0;
    private int b = 0;

    protected override SimpleColor CreateValue() {
        b = (b < 255) ? b + 1 : 0;
        g = (b == 0 && g < 255) ? g + 1 : g;
        r = (b == 0 && g == 0 && r < 255) ? r + 1 : r;

        return new SimpleColor((byte)r, (byte)g, (byte)b);
    }
}
