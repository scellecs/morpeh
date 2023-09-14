#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY
namespace Scellecs.Morpeh.Logging {
    internal class MorpehUnityLogger : IMorpehLogger {
        void IMorpehLogger.Log(string message)                      => UnityEngine.Debug.Log(message);
        void IMorpehLogger.LogWarning(string message)               => UnityEngine.Debug.LogWarning(message);
        void IMorpehLogger.LogError(string message)                 => UnityEngine.Debug.LogError(message);
        void IMorpehLogger.LogException(System.Exception exception) => UnityEngine.Debug.LogException(exception);
        void IMorpehLogger.BeginSample(string name)                 => UnityEngine.Profiling.Profiler.BeginSample(name);
        void IMorpehLogger.EndSample()                              => UnityEngine.Profiling.Profiler.EndSample();
    }
}
#endif