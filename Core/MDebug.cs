using Morpeh.Logging;
#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Morpeh {
    using System;
    using System.Diagnostics;
    
    internal static class MDebug {
        [Conditional("MORPEH_DEBUG")]
        public static void Log(object message) => MorpehSettings.Logger.Log($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogWarning(object message) => MorpehSettings.Logger.LogWarning($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogError(object message) => MorpehSettings.Logger.LogError($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogException(Exception e) => MorpehSettings.Logger.LogException(e);
    }
}
