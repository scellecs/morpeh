namespace Morpeh
{
    using Logging;
    
    public static class MorpehSettings {
#if UNITY_2019_1_OR_NEWER
        internal static IMorpehLogger Logger = new MorpehUnityLogger();
#else
        internal static IMorpehLogger Logger = new MorpehSystemLogger();
#endif
        
        public static void UseLogger(IMorpehLogger logger) {
            Logger = logger;
        }
    }
}