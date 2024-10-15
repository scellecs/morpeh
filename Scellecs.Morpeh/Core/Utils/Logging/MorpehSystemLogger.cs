namespace Scellecs.Morpeh.Logging  {
    internal class MorpehSystemLogger : IMorpehLogger {
        void IMorpehLogger.Log(string message) => System.Console.WriteLine(message);
        void IMorpehLogger.LogWarning(string message) => System.Console.WriteLine(message);
        void IMorpehLogger.LogError(string message) => System.Console.WriteLine(message);
        void IMorpehLogger.LogException(System.Exception exception) => System.Console.WriteLine(exception);
        void IMorpehLogger.LogTrace(string message) => System.Console.WriteLine(message);
        void IMorpehLogger.BeginSample(string name) {}
        void IMorpehLogger.EndSample() {}
    }
}