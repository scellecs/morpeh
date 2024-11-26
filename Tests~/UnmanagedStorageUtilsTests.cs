using Scellecs.Morpeh.Collections;

namespace Tests;

[Collection("Sequential")]
public unsafe class UnmanagedStorageUtilsTests {
    [Fact]
    public void AllocateUnsafeArray_CreatesArrayWithCorrectProperties() {
        var capacity = 10;
        var storage = new UnmanagedStorage();
        UnmanagedStorageUtils.AllocateUnsafeArray<int>(&storage, capacity);

        Assert.True(storage.IsCreated);
        Assert.Equal(capacity, storage.Capacity);
        Assert.Equal(0, storage.Length);
    }

    [Fact]
    public void DeallocateUnsafeArray_ReleasesResourcesCorrectly() {
        var capacity = 10;
        var storage = new UnmanagedStorage();
        UnmanagedStorageUtils.AllocateUnsafeArray<int>(&storage, capacity);
        UnmanagedStorageUtils.DeallocateUnsafeArray<int>(&storage);

        Assert.False(storage.IsCreated);
        Assert.Equal(0, storage.Capacity);
        Assert.Equal(0, storage.Length);
    }

    [Fact]
    public void DeallocateUnsafeArray_HandleNullOrNotCreatedArray() {
        var storage = new UnmanagedStorage();
        UnmanagedStorageUtils.DeallocateUnsafeArray<int>(&storage);
    }

    [Fact]
    public void ResizeUnsafeArray_IncreasesCapacity() {
        var initialCapacity = 5;
        var newCapacity = 10;
        var storage = new UnmanagedStorage();
        UnmanagedStorageUtils.AllocateUnsafeArray<int>(&storage, initialCapacity);

        for (int i = 0; i < initialCapacity; i++) {
            *((int*)storage.Ptr + i) = i;
            storage.Length++;
        }

        UnmanagedStorageUtils.ResizeUnsafeArray<int>(&storage, newCapacity);

        Assert.Equal(newCapacity, storage.Capacity);
        Assert.Equal(initialCapacity, storage.Length);

        for (int i = 0; i < initialCapacity; i++) {
            Assert.Equal(i, *((int*)storage.Ptr + i));
        }
    }

    [Fact]
    public void ResizeUnsafeArray_DecreasesCapacity() {
        var initialCapacity = 10;
        var newCapacity = 5;
        var storage = new UnmanagedStorage();
        UnmanagedStorageUtils.AllocateUnsafeArray<int>(&storage, initialCapacity);

        for (int i = 0; i < initialCapacity; i++) {
            *((int*)storage.Ptr + i) = i;
            storage.Length++;
        }

        UnmanagedStorageUtils.ResizeUnsafeArray<int>(&storage, newCapacity);

        Assert.Equal(newCapacity, storage.Capacity);
        Assert.Equal(newCapacity, storage.Length);

        for (int i = 0; i < newCapacity; i++) {
            Assert.Equal(i, *((int*)storage.Ptr + i));
        }
    }
}