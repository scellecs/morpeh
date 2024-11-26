using Scellecs.Morpeh.Collections;
using System.Numerics;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public abstract class UnmanagedListTests<T>(ITestOutputHelper output) where T : unmanaged {
    private readonly ITestOutputHelper output = output;

    protected abstract T CreateValue();

    [Fact]
    public void Create_CreatesListWithCorrectCapacity() {
        int expectedCapacity = 10;
        using var list = UnmanagedList<T>.Create(expectedCapacity);

        Assert.True(list.IsCreated);
        Assert.Equal(0, list.Length);
        Assert.Equal(expectedCapacity, list.Capacity);
    }

    [Fact]
    public void Indexer_GetSetWorksCorrectly() {
        var list = UnmanagedList<T>.Create(5);
        var testValue = CreateValue();

        list.Add(testValue);
        Assert.Equal(testValue, list[0]);

        var newValue = CreateValue();
        list[0] = newValue;
        Assert.Equal(newValue, list[0]);

        list.Dispose();
    }

    [Fact]
    public void Indexer_ThrowsOnUncreatedList() {
        var list = new UnmanagedList<T>();

        Assert.Throws<Exception>(() => { var value = list[0]; });
        Assert.Throws<Exception>(() => { list[0] = CreateValue(); });
    }

    [Fact]
    public void Indexer_ThrowsOnOutOfRangeIndex() {
        var list = UnmanagedList<T>.Create(5);

        Assert.Throws<IndexOutOfRangeException>(() => { var value = list[-1]; });
        Assert.Throws<IndexOutOfRangeException>(() => { var value = list[0]; });
        Assert.Throws<IndexOutOfRangeException>(() => { list[-1] = CreateValue(); });
        Assert.Throws<IndexOutOfRangeException>(() => { list[0] = CreateValue(); });

        list.Dispose();
    }

    [Fact]
    public void GetElementAsRef_WorksCorrectly() {
        using var list = UnmanagedList<T>.Create(5);
        var testValue = CreateValue();

        list.Add(CreateValue());
        list.Add(CreateValue());
        list.Add(testValue);

        ref var elementRef = ref list.GetElementAsRef(2);
        elementRef = testValue;

        Assert.Equal(testValue, list[2]);
    }

    [Fact]
    public void GetElementAsRef_ThrowsOnUnvalidConditions() {
        var list = new UnmanagedList<T>();

        Assert.Throws<Exception>(() => { ref var elementRef = ref list.GetElementAsRef(0); });

        using var validList = UnmanagedList<T>.Create(5);
        Assert.Throws<IndexOutOfRangeException>(() => { ref var elementRef = ref validList.GetElementAsRef(-1); });
        Assert.Throws<IndexOutOfRangeException>(() => { ref var elementRef = ref validList.GetElementAsRef(5); });
    }

    [Fact]
    public void Add_IncreasesCapacityWhenNeeded() {
        using var list = UnmanagedList<T>.Create(2);
        var testValues = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        foreach (var value in testValues) {
            list.Add(value);
        }

        Assert.Equal(4, list.Length);
        Assert.True(list.Capacity >= 4);

        for (int i = 0; i < testValues.Length; i++) {
            Assert.Equal(testValues[i], list[i]);
        }
    }

    [Fact]
    public void RemoveAt_RemovesElementCorrectly() {
        using var list = UnmanagedList<T>.Create(5);
        var testValues = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        foreach (var value in testValues) {
            list.Add(value);
        }

        list.RemoveAt(1);

        Assert.Equal(3, list.Length);
        Assert.Equal(testValues[0], list[0]);
        Assert.Equal(testValues[2], list[1]);
        Assert.Equal(testValues[3], list[2]);
    }

    [Fact]
    public void RemoveAtSwapBack_RemovesElementAndSwapsWithLast() {
        using var list = UnmanagedList<T>.Create(5);
        var testValues = new[] { CreateValue(), CreateValue(), CreateValue(), CreateValue() };

        foreach (var value in testValues) {
            list.Add(value);
        }

        list.RemoveAtSwapBack(1);

        Assert.Equal(3, list.Length);
        Assert.Equal(testValues[0], list[0]);
        Assert.Equal(testValues[3], list[1]);
        Assert.Equal(testValues[2], list[2]);
    }

    [Fact]
    public void Clear_ResetsListToZeroLength() {
        using var list = UnmanagedList<T>.Create(5);

        for (int i = 0; i < 3; i++)
        {
            list.Add(CreateValue());
        }

        list.Clear();

        Assert.Equal(0, list.Length);
        Assert.Equal(5, list.Capacity);
    }

    [Fact]
    public void ForEach_IteratesOverAllElements() {
        using var list = UnmanagedList<T>.Create(5);
        var testValues = new T[5];

        for (int i = 0; i < 5; i++) {
            testValues[i] = CreateValue();
            list.Add(testValues[i]);
        }

        int index = 0;
        foreach (var value in list) {
            Assert.Equal(testValues[index++], value);
        }

        Assert.Equal(5, index);
    }

    [Fact]
    public void ForEach_ThrowsOnUncreatedList() {
        var list = new UnmanagedList<T>();

        Assert.Throws<Exception>(() => {
            foreach (var _ in list) {
            }
        });
    }

    [Fact]
    public void For_IterationWithRemovalReverseLoop() {
        using var list = UnmanagedList<T>.Create(50);
        var values = new List<T>();

        for (int i = 0; i < 50; i++) {
            var value = CreateValue();
            values.Add(value);
            list.Add(value);
        }

        for (int i = list.Length - 1; i >= 0; i--) {
            if (i % 3 == 2) {
                list.RemoveAt(i);
                values.RemoveAt(i);
            }
        }

        Assert.Equal(values.Count, list.Length);

        for (int i = 0; i < list.Length; i++) {
            Assert.Equal(values[i], list[i]);
        }
    }

    [Fact]
    public void Dispose_ReleasesResources() {
        var list = UnmanagedList<T>.Create(5);
        Assert.True(list.IsCreated);
        Assert.True(list.Capacity > 0);
        Assert.True(list.Length >= 0);

        list.Dispose();

        Assert.False(list.IsCreated);
        Assert.False(list.Length >= 0);
    }
}

public class UnmanagedListIntTests(ITestOutputHelper output) : UnmanagedListTests<int>(output) {
    private int value = 0;

    protected override int CreateValue() {
        return this.value++;
    }
}

public class UnmanagedListVector2Tests(ITestOutputHelper output) : UnmanagedListTests<Vector2>(output) {
    private int nextCoord = 0;

    protected override Vector2 CreateValue() {
        var coord = nextCoord++;
        return new Vector2(coord, coord * 2);
    }
}