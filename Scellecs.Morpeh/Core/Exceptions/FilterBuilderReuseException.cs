namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;

    public class FilterBuilderReuseException : Exception {
        private FilterBuilderReuseException(string message) : base(message) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw() {
            throw new FilterBuilderReuseException("[MORPEH] FilterBuilder has already been built or has been reused in a separate branch. Use a new FilterBuilder to create a new filter.");
        }
    }
}