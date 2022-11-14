namespace Scellecs.Morpeh.Logging {
    public interface IMorpehLogger {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(System.Exception exception);
        void BeginSample(string name);
        void EndSample();
    }
}
