using System;
using System.Runtime.CompilerServices;
namespace Scellecs.Morpeh {
    public static class SystemExceptionUtility {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException(string message) {
            throw new ArgumentException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException(string parameterName, string message) {
            throw new ArgumentException(message, parameterName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException(string parameterName) {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException(string parameterName, string message) {
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }
}