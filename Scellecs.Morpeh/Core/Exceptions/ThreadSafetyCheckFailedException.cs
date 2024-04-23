namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class ThreadSafetyCheckFailedException : Exception {
        private ThreadSafetyCheckFailedException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw(int currentThread, int expectedThread) {
            throw new ThreadSafetyCheckFailedException($"[MORPEH] Thread safety check failed. You are trying touch the world from a thread {currentThread}, but the world associated with the thread {expectedThread}");
        }
    }
}