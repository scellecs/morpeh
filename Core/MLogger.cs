namespace Morpeh {
    using Logging;
    
    public static class MLogger {
#if UNITY_2019_1_OR_NEWER
        internal static IMorpehLogger Instance = new MorpehUnityLogger();
#else
        internal static IMorpehLogger Instance = new MorpehSystemLogger();
#endif
        
        public static void SetInstance(IMorpehLogger logger) {
            Instance = logger;
        }
    }
}