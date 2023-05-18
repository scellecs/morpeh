namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class HashHelpers {
        internal static readonly int[] capacitySizes = {
            3,
            15,
            63,
            255,
            1_023,
            4_095,
            16_383,
            65_535,
            131_071,
            262_143,
            524_287,
            1_048_575,
            2_097_151,
            4_194_303,
            8_388_607,
            16_777_215,
            33_554_431,
            67_108_863,
            134_217_727,
            268_435_455,
            536_870_912,
            1_073_741_823
        };
            
        internal static readonly int[] smallCapacitySizes = {
            3,
            7,
            15,
            31,
            63,
            127,
            255,
            511,
            1_023,
            2_047,
            4_095,
            16_383,
            65_535,
            131_071,
            262_143,
            524_287,
            1_048_575,
            2_097_151,
            4_194_303,
            8_388_607,
            16_777_215,
            33_554_431,
            67_108_863,
            134_217_727,
            268_435_455,
            536_870_912,
            1_073_741_823
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCapacity(int min) {
            for (int index = 0, length = capacitySizes.Length; index < length; ++index) {
                var prime = capacitySizes[index];
                if (prime >= min) {
                    return prime;
                }
            }

            throw new Exception("Capacity is too big");
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCapacitySmall(int min) {
            for (int index = 0, length = smallCapacitySizes.Length; index < length; ++index) {
                var prime = smallCapacitySizes[index];
                if (prime >= min) {
                    return prime;
                }
            }

            throw new Exception("Capacity is too big");
        }
    }
}