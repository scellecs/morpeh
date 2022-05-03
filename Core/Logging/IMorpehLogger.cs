namespace Morpeh.Logging
{
    public interface IMorpehLogger
    {
        public void Log(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
        public void LogException(System.Exception exception) { }
    }
}