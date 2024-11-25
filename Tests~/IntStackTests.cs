using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class IntStackTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Push_AddsElementCorrectly() {
        var stack = new IntStack();
        stack.Push(42);

        Assert.Equal(1, stack.length);
        Assert.Equal(42, stack.data[0]);
    }

    [Fact]
    public void Push_ExpandsCapacityWhenNeeded() {
        var stack = new IntStack();

        for (int i = 0; i < 4; i++) {
            stack.Push(i);
        }

        var oldCapacity = stack.capacity;

        for (int i = 4; i < 1000; i++) {
            stack.Push(i);
        }

        Assert.True(stack.capacity > oldCapacity);
        Assert.Equal(1000, stack.length);

        for (int i = 0; i < 1000; i++) {
            Assert.Equal(i, stack.data[i]);
        }
    }

    [Fact]
    public void Pop_ReturnsLastElement() {
        var stack = new IntStack();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        Assert.Equal(3, stack.Pop());
        Assert.Equal(2, stack.length);
        Assert.Equal(2, stack.Pop());
        Assert.Equal(1, stack.length);
        Assert.Equal(1, stack.Pop());
        Assert.Equal(0, stack.length);
    }

    [Fact]
    public void TryPop_ReturnsTrueAndValueWhenNotEmpty() {
        var stack = new IntStack();
        stack.Push(42);

        var result = stack.TryPop(out int value);

        Assert.True(result);
        Assert.Equal(42, value);
        Assert.Equal(0, stack.length);
    }

    [Fact]
    public void TryPop_ReturnsFalseWhenEmpty() {
        var stack = new IntStack();
        var result = stack.TryPop(out int value);

        Assert.False(result);
        Assert.Equal(default, value);
        Assert.Equal(0, stack.length);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(10000)]
    public void PushPop_HandlesMultipleOperations(int operationsCount) {
        var stack = new IntStack();
        var reference = new Stack<int>();
        var random = new Random(42);

        for (int i = 0; i < operationsCount; i++) {
            var value = random.Next();
            stack.Push(value);
            reference.Push(value);
        }

        Assert.Equal(reference.Count, stack.length);

        while (reference.Count > 0) {
            Assert.Equal(reference.Pop(), stack.Pop());
        }

        Assert.Equal(0, stack.length);
    }

    [Fact]
    public void Clear_RemovesAllElements() {
        var stack = new IntStack();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        var oldCapacity = stack.capacity;

        stack.Clear();

        Assert.Equal(0, stack.length);
        Assert.Equal(oldCapacity, stack.capacity);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    public void Stress_PushPopOperations(int operationsCount) {
        var stack = new IntStack();
        var reference = new Stack<int>();
        var random = new Random(42);

        for (int i = 0; i < operationsCount; i++) {
            var operation = random.NextSingle();

            if (operation < 0.7f || reference.Count == 0) {
                var value = random.Next();
                stack.Push(value);
                reference.Push(value);
            }
            else {
                Assert.Equal(reference.Pop(), stack.Pop());
            }

            Assert.Equal(reference.Count, stack.length);
        }

        Assert.Equal(reference.Count, stack.length);
        while (reference.Count > 0) {
            Assert.Equal(reference.Pop(), stack.Pop());
        }
    }
}