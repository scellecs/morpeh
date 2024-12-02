#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using System;

namespace Scellecs.Morpeh.Benchmarks.Collections {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BenchmarkComparisonAttribute : Attribute {
        public Type EnumType { get; }
        public string[] Names { get; }

        internal BenchmarkComparisonAttribute(Type enumType, params string[] names) {
            if (!enumType.IsEnum) {
                throw new ArgumentException("Type must be an enum", nameof(enumType));
            }

            if (names == null || names.Length != Enum.GetValues(enumType).Length) {
                throw new ArgumentException($"Must provide names for all enum values. Expected: {Enum.GetValues(enumType).Length}, Got: {names?.Length ?? 0}");
            }

            this.EnumType = enumType;
            this.Names = names;
        }

        public string GetNameForValue(Enum value) {
            if (value.GetType() != this.EnumType) {
                throw new ArgumentException($"Invalid enum type. Expected {this.EnumType}, got {value.GetType()}");
            }

            return this.Names[Convert.ToInt32(value)];
        }
    }
}
#endif
