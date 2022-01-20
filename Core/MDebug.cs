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
        public static void Log(object message) => UnityEngine.Debug.Log($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogWarning(object message) => UnityEngine.Debug.LogWarning($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogError(object message) => UnityEngine.Debug.LogError($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogException(Exception e) => UnityEngine.Debug.LogException(e);
        

    }
}
