using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

public unsafe abstract class PinnedArrayTests<T>(ITestOutputHelper output) where T : unmanaged {
    private readonly ITestOutputHelper output = output;

    protected abstract T CreateValue();

    [Fact]
    public void Constructor_InitializesArrayWithSpecifiedSize() {
        var array = new PinnedArray<T>(10);

        Assert.NotNull(array.data);
        Assert.Equal(10, array.data.Length);
        Assert.NotEqual(IntPtr.Zero, new IntPtr(array.ptr));

        array.Dispose();
    }

    [Fact]
    public void Clear_SetsAllElementsToZero() {
        var array = new PinnedArray<T>(10);
        for (int i = 0; i < 10; i++) {
            array.data[i] = CreateValue();
        }

        array.Clear();

        foreach (var value in array.data) {
            Assert.Equal(default, value);
        }

        array.Dispose();
    }

    [Fact]
    public void Enumerator_IteratesOverAllElements() {
        var array = new PinnedArray<T>(5);
        for (int i = 0; i < 5; i++) {
            array.data[i] = CreateValue();
        }

        var enumerator = array.GetEnumerator();
        int index = 0;

        while (enumerator.MoveNext()) {
            Assert.Equal(array.data[index], enumerator.Current);
            index++;
        }

        Assert.Equal(5, index);

        array.Dispose();
    }

    [Fact]
    public void Enumerator_HandlesEmptyArray() {
        var array = new PinnedArray<T>(0);

        var enumerator = array.GetEnumerator();
        Assert.False(enumerator.MoveNext());

        array.Dispose();
    }

    [Fact]
    public void Constructor_PtrPointsToDataStart() {
        var array = new PinnedArray<T>(10);

        for (int i = 0; i < 10; i++) {
            array.data[i] = CreateValue();
        }

        for (int i = 0; i < 10; i++) {
            Assert.Equal(array.data[i], array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_UpdatesPtrToNewArray() {
        var array = new PinnedArray<T>(5);
        var list = new List<T>();

        for (int i = 0; i < 5; i++) {
            var value = CreateValue();
            array.data[i] = value;
            list.Add(value);
        }

        var oldPtr = array.ptr;

        array.Resize(10);

        Assert.NotEqual((IntPtr)oldPtr, (IntPtr)array.ptr);
        Assert.Equal(10, array.data.Length);

        for (int i = 0; i < 5; i++) {
            Assert.Equal(list[i], array.ptr[i]);
        }

        for (int i = 5; i < 10; i++) {
            Assert.Equal(default, array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_ChangesArraySizeAndPreservesData() {
        var array = new PinnedArray<T>(5);
        var list = new List<T>();
        for (int i = 0; i < 5; i++) {
            var value = CreateValue();
            array.data[i] = value;
            list.Add(value);
        }

        array.Resize(10);

        Assert.Equal(10, array.data.Length);
        for (int i = 0; i < 5; i++) {
            Assert.Equal(list[i], array.data[i]);
        }

        for (int i = 5; i < 10; i++) {
            Assert.Equal(default, array.data[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_ShrinksArrayAndTruncatesData() {
        var array = new PinnedArray<T>(10);
        var list = new List<T>();
        for (int i = 0; i < 10; i++) {
            var value = CreateValue();
            array.data[i] = value;
            list.Add(value);
        }

        array.Resize(5);

        Assert.Equal(5, array.data.Length);
        for (int i = 0; i < 5; i++) {
            Assert.Equal(list[i], array.data[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_EmptyToNonEmpty() {
        var array = new PinnedArray<T>(0);
        array.Resize(10);

        Assert.Equal(10, array.data.Length);
        for (int i = 0; i < 10; i++) {
            Assert.Equal(default, array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_NonEmptyToEmpty() {
        var array = new PinnedArray<T>(10);
        array.Resize(0);

        Assert.Empty(array.data);
        array.Dispose();
    }

    [Fact]
    public void Clear_ResetsValuesPointedByPtr() {
        var array = new PinnedArray<T>(10);

        for (int i = 0; i < 10; i++) {
            array.data[i] = CreateValue();
        }

        array.Clear();

        for (int i = 0; i < 10; i++) {
            Assert.Equal(default, array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void PtrReflectsDataModifications() {
        var array = new PinnedArray<T>(5);

        for (int i = 0; i < 5; i++) {
            array.ptr[i] = CreateValue();
        }

        for (int i = 0; i < 5; i++) {
            Assert.Equal(array.ptr[i], array.data[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Dispose_ReleasesHandleAndNullifiesReferences() {
        var array = new PinnedArray<T>(10);
        array.Dispose();

        Assert.Null(array.data);
        Assert.Equal(IntPtr.Zero, new IntPtr(array.ptr));
    }

    [Fact]
    public void DoubleDispose_DoesNotThrowException() {
        var array = new PinnedArray<T>(10);
        array.Dispose();
        var exception = Record.Exception(() => array.Dispose());
        Assert.Null(exception);
    }
}

public class PinnedArrayIntTests(ITestOutputHelper output) : PinnedArrayTests<int>(output) {
    private int value = 0;

    protected override int CreateValue() {
        return this.value++;
    }
}

public class PinnedArrayEntityTests(ITestOutputHelper output) : PinnedArrayTests<Entity>(output) {
    private int next = 0;

    protected override Entity CreateValue() {
        return new Entity(0, 0, ++next, 0);
    }
}
