#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_DEBUG_VERBOSE
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Morpeh {
    using System.Diagnostics;
    
    internal static class MDebug {
        [Conditional("MORPEH_DEBUG_VERBOSE")]
        public static void LogVerbose(object message) => UnityEngine.Debug.Log($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void LogError(object message) => UnityEngine.Debug.Log($"[MORPEH] {message}");
    }
}
