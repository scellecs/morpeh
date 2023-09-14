#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Diagnostics;
    using Logging;

    public static class MLogger {
#if MORPEH_UNITY
        internal static IMorpehLogger Instance = new MorpehUnityLogger();
#else
        internal static IMorpehLogger Instance = new MorpehSystemLogger();
#endif
        
        public static void SetInstance(IMorpehLogger logger) {
            Instance = logger;
        }
        
        [Conditional("MORPEH_DEBUG")]
        public static void Log(object message) => Instance.Log($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogWarning(object message) => Instance.LogWarning($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogError(object message) => Instance.LogError($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogException(Exception e) => Instance.LogException(e);
        
        [Conditional("MORPEH_PROFILING")]
        public static void BeginSample(string name) => Instance.BeginSample(name);
        [Conditional("MORPEH_PROFILING")]
        public static void EndSample() => Instance.EndSample();
    }
}