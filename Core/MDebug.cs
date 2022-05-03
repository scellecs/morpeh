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
        public static void Log(object message) => MLogger.Instance.Log($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogWarning(object message) => MLogger.Instance.LogWarning($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogError(object message) => MLogger.Instance.LogError($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogException(Exception e) => MLogger.Instance.LogException(e);
    }
}
