namespace Morpeh.Logging
{
    internal class MorpehSystemLogger : IMorpehLogger
    {
        public void Log(string message) => System.Console.WriteLine(message);
        public void LogWarning(string message) => System.Console.WriteLine(message);
        public void LogError(string message) => System.Console.WriteLine(message);
        public void LogException(System.Exception exception) => System.Console.WriteLine(exception);
    }
}