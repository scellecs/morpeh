using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class ArrayHelpersTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Grow_IncreasesArraySize() {
        var array = new int[] { 1, 2, 3 };
        var newSize = 6;

        ArrayHelpers.Grow(ref array, newSize);
        Assert.Equal(newSize, array.Length);
        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
        Assert.Equal(0, array[3]);
    }

    [Fact]
    public void Grow_PreservesOriginalElements() {
        var array = new string[] { "a", "b", "c" };
        var newSize = 5;

        ArrayHelpers.Grow(ref array, newSize);
        Assert.Equal("a", array[0]);
        Assert.Equal("b", array[1]);
        Assert.Equal("c", array[2]);
        Assert.Null(array[3]);
        Assert.Null(array[4]);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Grow_HandlesVariousSizes(int newSize) {
        var array = new int[] { 1, 2, 3, 4, 5 };
        var originalLength = array.Length;

        ArrayHelpers.Grow(ref array, newSize);
        Assert.Equal(newSize, array.Length);
        for (int i = 0; i < originalLength; i++) {
            Assert.Equal(i + 1, array[i]);
        }
    }

    [Fact]
    public void GrowNonInlined_IncreasesArraySize() {
        var array   = new int[] { 1, 2, 3 };
        var newSize = 6;

        ArrayHelpers.GrowNonInlined(ref array, newSize);
        Assert.Equal(newSize, array.Length);
        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
        Assert.Equal(0, array[3]);
    }

    [Fact]
    public void GrowNonInlined_PreservesOriginalElements() {
        var array   = new string[] { "a", "b", "c" };
        var newSize = 5;

        ArrayHelpers.GrowNonInlined(ref array, newSize);
        Assert.Equal("a", array[0]);
        Assert.Equal("b", array[1]);
        Assert.Equal("c", array[2]);
        Assert.Null(array[3]);
        Assert.Null(array[4]);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void GrowNonInlined_HandlesVariousSizes(int newSize) {
        var array          = new int[] { 1, 2, 3, 4, 5 };
        var originalLength = array.Length;

        ArrayHelpers.GrowNonInlined(ref array, newSize);
        Assert.Equal(newSize, array.Length);
        for (int i = 0; i < originalLength; i++) {
            Assert.Equal(i + 1, array[i]);
        }
    }

    [Fact]
    public void IndexOf_FindsExistingElement() {
        var array = new int[] { 1, 2, 3, 4, 5 };
        var comparer = EqualityComparer<int>.Default;
        var index = ArrayHelpers.IndexOf(array, 3, comparer);

        Assert.Equal(2, index);
    }

    [Fact]
    public void IndexOf_ReturnsMinusOneForNonExistingElement() {
        var array = new int[] { 1, 2, 3, 4, 5 };
        var comparer = EqualityComparer<int>.Default;
        var index = ArrayHelpers.IndexOf(array, 10, comparer);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_HandlesEmptyArray() {
        var array = Array.Empty<int>();
        var comparer = EqualityComparer<int>.Default;
        var index = ArrayHelpers.IndexOf(array, 1, comparer);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_WorksWithStrings() {
        var array = new string[] { "apple", "banana", "orange" };
        var searchValue = "banana";
        var comparer = EqualityComparer<string>.Default;
        var index = ArrayHelpers.IndexOf(array, searchValue, comparer);

        Assert.Equal(1, index);
    }

#pragma warning disable 8625, 8620, 8600
    [Fact]
    public void IndexOf_HandlesNullValues() {
        var array = new string[] { "apple", null, "orange" };
        string searchValue = null;
        var comparer = EqualityComparer<string>.Default;
        var index = ArrayHelpers.IndexOf(array, searchValue, comparer);

        Assert.Equal(1, index);
    }
#pragma warning restore 8625, 8620, 8600

    [Fact]
    public unsafe void IndexOfUnsafeInt_FindsExistingElement() {
        var array = new int[] { 1, 2, 3, 4, 5 };
        fixed (int* ptr = array) {
            var index = ArrayHelpers.IndexOfUnsafeInt(ptr, array.Length, 3);
            Assert.Equal(2, index);
        }
    }

    [Fact]
    public unsafe void IndexOfUnsafeInt_ReturnsMinusOneForNonExistingElement() {
        var array = new int[] { 1, 2, 3, 4, 5 };
        fixed (int* ptr = array) {
            var index = ArrayHelpers.IndexOfUnsafeInt(ptr, array.Length, 10);
            Assert.Equal(-1, index);
        }
    }

    [Fact]
    public unsafe void IndexOfUnsafeInt_HandlesEmptyArray() {
        var array = Array.Empty<int>();
        fixed (int* ptr = array) {
            var index = ArrayHelpers.IndexOfUnsafeInt(ptr, array.Length, 1);
            Assert.Equal(-1, index);
        }
    }
}