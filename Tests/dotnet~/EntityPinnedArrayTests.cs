using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public unsafe class EntityPinnedArrayTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Constructor_InitializesArrayWithSpecifiedSize() {
        var array = new EntityPinnedArray(10);

        Assert.NotNull(array.data);
        Assert.Equal(10, array.data.Length);
        Assert.NotEqual(IntPtr.Zero, new IntPtr(array.ptr));

        array.Dispose();
    }

    [Fact]
    public void Clear_SetsAllElementsToDefault() {
        var array = new EntityPinnedArray(10);
        for (int i = 0; i < 10; i++) {
            array.data[i] = new Entity(0, 0, i + 1, 0);
        }

        array.Clear();

        foreach (var value in array.data) {
            Assert.Equal(default, value);
        }

        array.Dispose();
    }

    [Fact]
    public void Constructor_PtrPointsToDataStart() {
        var array = new EntityPinnedArray(10);

        for (int i = 0; i < 10; i++) {
            array.data[i] = new Entity(0, 0, i + 1, 0);
        }

        for (int i = 0; i < 10; i++) {
            Assert.Equal(array.data[i], array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_UpdatesPtrToNewArray() {
        var array = new EntityPinnedArray(5);

        for (int i = 0; i < 5; i++) {
            array.data[i] = new Entity(0, 0, i + 1, 0);
        }

        var oldPtr = array.ptr;

        array.Resize(10);

        Assert.NotEqual((IntPtr)oldPtr, (IntPtr)array.ptr);
        Assert.Equal(10, array.data.Length);

        for (int i = 0; i < 5; i++) {
            Assert.Equal(new Entity(0, 0, i + 1, 0), array.ptr[i]);
        }

        for (int i = 5; i < 10; i++) {
            Assert.Equal(default, array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_ChangesArraySizeAndPreservesData() {
        var array = new EntityPinnedArray(5);
        for (int i = 0; i < 5; i++) {
            array.data[i] = new Entity(0, 0, i + 1, 0);
        }

        array.Resize(10);

        Assert.Equal(10, array.data.Length);
        for (int i = 0; i < 5; i++) {
            Assert.Equal(new Entity(0, 0, i + 1, 0), array.data[i]);
        }

        for (int i = 5; i < 10; i++) {
            Assert.Equal(default, array.data[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_ShrinksArrayAndTruncatesData() {
        var array = new EntityPinnedArray(10);
        for (int i = 0; i < 10; i++) {
            array.data[i] = new Entity(0, 0, i + 1, 0);
        }

        array.Resize(5);

        Assert.Equal(5, array.data.Length);
        for (int i = 0; i < 5; i++) {
            Assert.Equal(new Entity(0, 0, i + 1, 0), array.data[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_EmptyToNonEmpty() {
        var array = new EntityPinnedArray(0);
        array.Resize(10);

        Assert.Equal(10, array.data.Length);
        for (int i = 0; i < 10; i++) {
            Assert.Equal(default, array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Resize_NonEmptyToEmpty() {
        var array = new EntityPinnedArray(10);
        array.Resize(0);

        Assert.Empty(array.data);
        array.Dispose();
    }

    [Fact]
    public void Clear_ResetsValuesPointedByPtr() {
        var array = new EntityPinnedArray(10);

        for (int i = 0; i < 10; i++) {
            array.data[i] = new Entity(0, 0, i + 1, 0);
        }

        array.Clear();

        for (int i = 0; i < 10; i++) {
            Assert.Equal(default, array.ptr[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void PtrReflectsDataModifications() {
        var array = new EntityPinnedArray(5);

        for (int i = 0; i < 5; i++) {
            array.ptr[i] = new Entity(0, 0, (i + 1) * 10, 0);
        }

        for (int i = 0; i < 5; i++) {
            Assert.Equal(new Entity(0, 0, (i + 1) * 10, 0), array.data[i]);
        }

        array.Dispose();
    }

    [Fact]
    public void Dispose_ReleasesHandleAndNullifiesReferences() {
        var array = new EntityPinnedArray(10);
        array.Dispose();

        Assert.Null(array.data);
        Assert.Equal(IntPtr.Zero, new IntPtr(array.ptr));
    }

    [Fact]
    public void DoubleDispose_DoesNotThrowException() {
        var array = new EntityPinnedArray(10);
        array.Dispose();
        var exception = Record.Exception(() => array.Dispose());
        Assert.Null(exception);
    }
}