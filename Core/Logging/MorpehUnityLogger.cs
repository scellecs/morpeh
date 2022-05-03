#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Logging
{
    internal class MorpehUnityLogger : IMorpehLogger
    {
        public void Log(string message) => UnityEngine.Debug.Log(message);
        public void LogWarning(string message) => UnityEngine.Debug.LogWarning(message);
        public void LogError(string message) => UnityEngine.Debug.LogError(message);
        public void LogException(System.Exception exception) => UnityEngine.Debug.LogException(exception);
    }
}
#endif