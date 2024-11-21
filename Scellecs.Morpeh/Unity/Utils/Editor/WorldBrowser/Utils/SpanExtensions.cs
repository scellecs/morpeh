#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using System;
namespace Scellecs.Morpeh.Utils.Editor{
    internal static class SpanExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpanSplitter<T> Split<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> separator) where T : IEquatable<T> {
            return new SpanSplitter<T>(source, separator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpanSplitter<T> Split<T>(this Span<T> source, ReadOnlySpan<T> separator) where T : IEquatable<T> {
            return new SpanSplitter<T>(source, separator);
        }
    }

    internal readonly ref struct SpanSplitter<T> where T : IEquatable<T> {
        private readonly ReadOnlySpan<T> source;
        private readonly ReadOnlySpan<T> separator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanSplitter(ReadOnlySpan<T> source, ReadOnlySpan<T> separator) {
            if (0 == separator.Length) {
                throw new ArgumentException("Requires non-empty value", nameof(separator));
            }

            this.source = source;
            this.separator = separator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanSplitEnumerator<T> GetEnumerator() {
            return new SpanSplitEnumerator<T>(this.source, this.separator);
        }
    }

    internal ref struct SpanSplitEnumerator<T> where T : IEquatable<T> {
        private int nextStartIndex;
        private readonly ReadOnlySpan<T> separator;
        private readonly ReadOnlySpan<T> source;
        private SpanSplitValue current;

        public SpanSplitEnumerator(ReadOnlySpan<T> source, ReadOnlySpan<T> separator) {
            this.nextStartIndex = 0;
            this.source = source;
            this.separator = separator;
            this.current = default;

            if (this.separator.Length == 0) {
                throw new ArgumentException("Requires non-empty value", nameof(this.separator));
            }
        }

        public bool MoveNext() {
            if (this.nextStartIndex > this.source.Length) {
                return false;
            }

            var nextSource = this.source[this.nextStartIndex..];
            var foundIndex = nextSource.IndexOf(this.separator);
            var length = foundIndex < 0 ? nextSource.Length : foundIndex;
            this.current = new SpanSplitValue(this.nextStartIndex, length, this.source);
            this.nextStartIndex += this.separator.Length + this.current.length;
            return true;
        }

        public readonly SpanSplitValue Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.current;
        }

        public readonly ref struct SpanSplitValue {
            public readonly int startIndex;
            public readonly int length;
            public readonly ReadOnlySpan<T> source;

            public SpanSplitValue(int startIndex, int length, ReadOnlySpan<T> source) {
                this.startIndex = startIndex;
                this.length = length;
                this.source = source;
            }

            public readonly ReadOnlySpan<T> AsSpan() => this.source.Slice(this.startIndex, this.length);

            public static implicit operator ReadOnlySpan<T>(SpanSplitValue value) => value.AsSpan();
        }
    }
}
#endif
