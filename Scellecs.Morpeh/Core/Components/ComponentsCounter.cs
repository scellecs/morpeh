namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ComponentsCounter {
        private static int value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Increment() {
            return Interlocked.Increment(ref value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Get() {
            return value;
        }
    }
}