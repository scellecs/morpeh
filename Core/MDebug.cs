#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Morpeh {
    using System.Diagnostics;
    
    internal static class MDebug {
        [Conditional("MORPEH_DEBUG")]
        public static void LogError(object message) => UnityEngine.Debug.LogError($"[MORPEH] {message}");
        
        [Conditional("MORPEH_DEBUG")]
        public static void Log(object message) => UnityEngine.Debug.Log($"[MORPEH] {message}");
    }
}
