using Scellecs.Morpeh.Collections;
using System.Numerics;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public unsafe abstract class UnmanagedArrayTests<T>(ITestOutputHelper output) where T : unmanaged {
    private readonly ITestOutputHelper output = output;

    protected abstract T CreateValue();

    [Fact]
    public void Create_CreatesArrayWithCorrectLength() {
        int expectedLength = 10;
        using var array = UnmanagedArray<T>.Create(expectedLength);

        Assert.True(array.IsCreated);
        Assert.Equal(expectedLength, array.Length);
    }

    [Fact]
    public void Indexer_GetSet_WorksCorrectly() {
        var array = UnmanagedArray<T>.Create(5);
        var testValue = CreateValue();
        array[2] = testValue;

        Assert.Equal(testValue, array[2]);
        array.Dispose();
    }

    [Fact]
    public void Indexer_ThrowsOnUncreatedArray() {
        var array = new UnmanagedArray<T>();

        Assert.Throws<Exception>(() => { var value = array[0]; });
        Assert.Throws<Exception>(() => { array[0] = CreateValue(); });
    }

    [Fact]
    public void Indexer_ThrowsOnOutOfRangeIndex() {
        var array = UnmanagedArray<T>.Create(5);

        Assert.Throws<IndexOutOfRangeException>(() => { var value = array[-1]; });
        Assert.Throws<IndexOutOfRangeException>(() => { var value = array[5]; });
        Assert.Throws<IndexOutOfRangeException>(() => { array[-1] = CreateValue(); });
        Assert.Throws<IndexOutOfRangeException>(() => { array[5] = CreateValue(); });
        array.Dispose();
    }

    [Fact]
    public void GetElementAsRef_WorksCorrectly() {
        using var array = UnmanagedArray<T>.Create(5);
        var testValue = CreateValue();

        ref var elementRef = ref array.GetElementAsRef(2);
        elementRef = testValue;

        Assert.Equal(testValue, array[2]);
    }

    [Fact]
    public void GetElementAsRef_ThrowsOnUnvalidConditions() {
        using var array = new UnmanagedArray<T>();

        Assert.Throws<Exception>(() => { ref var elementRef = ref array.GetElementAsRef(0); });
        Assert.Throws<IndexOutOfRangeException>(() => {
            using var validArray = UnmanagedArray<T>.Create(5);
            ref var elementRef = ref validArray.GetElementAsRef(-1);
        });
        Assert.Throws<IndexOutOfRangeException>(() => {
            using var validArray = UnmanagedArray<T>.Create(5);
            ref var elementRef = ref validArray.GetElementAsRef(5);
        });
    }

    [Fact]
    public void Resize_ChangesArrayLength() {
        using var array = UnmanagedArray<T>.Create(5);
        var originalValues = new T[5];
        for (int i = 0; i < 5; i++) {
            originalValues[i] = array[i];
        }

        array.Resize(10);

        Assert.Equal(10, array.Length);
        for (int i = 0; i < 5; i++) {
            Assert.Equal(originalValues[i], array[i]);
        }
    }

    [Fact]
    public void Resize_ThrowsOnUncreatedArray() {
        var array = new UnmanagedArray<T>();
        Assert.Throws<Exception>(() => { array.Resize(10); });
    }

    [Fact]
    public void Enumerator_WorksCorrectly() {
        var array = UnmanagedArray<T>.Create(5);
        var testValues = new T[5];
        for (int i = 0; i < 5; i++) {
            testValues[i] = CreateValue();
            array[i] = testValues[i];
        }

        int index = 0;
        foreach (var value in array) {
            Assert.Equal(testValues[index++], value);
        }

        Assert.Equal(5, index);
        array.Dispose();
    }

    [Fact]
    public void Enumerator_ThrowsOnUncreatedArray() {
        var array = new UnmanagedArray<T>();

        Assert.Throws<Exception>(() => {
            foreach (var _ in array) {
            }
        });
    }

    [Fact]
    public void Dispose_ReleasesResources() {
        var array = UnmanagedArray<T>.Create(5);
        Assert.True(array.IsCreated);
        Assert.True(array.Length >= 0);

        array.Dispose();
        Assert.False(array.IsCreated);
        Assert.False(array.Length >= 0);
    }
}

public class UnmanagedArrayIntTests(ITestOutputHelper output) : UnmanagedArrayTests<int>(output) {
    private int value = 0;

    protected override int CreateValue() {
        return this.value++;
    }
}

public class UnmanagedArrayVector2Tests(ITestOutputHelper output) : UnmanagedArrayTests<Vector2>(output) {
    private int nextCoord = 0;

    protected override Vector2 CreateValue() {
        var coord = nextCoord++;
        return new Vector2(coord, coord * 2);
    }
}