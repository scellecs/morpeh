using Scellecs.Morpeh.Logging;
using Xunit.Abstractions;

namespace Tests;

public class XUnitLogger : IMorpehLogger {
    private readonly ITestOutputHelper _output;

    public XUnitLogger(ITestOutputHelper output) {
        _output = output;
    }
    
    private string GetLogPrefix() {
        return $"[{DateTime.Now:HH:mm:ss.fff}]";
    }
    
    public void Log(string message) {
        _output.WriteLine($"{GetLogPrefix()} [LOG] {message}");
    }

    public void LogWarning(string message) {
        _output.WriteLine($"{GetLogPrefix()} [WARNING] {message}");
    }

    public void LogError(string message) {
        _output.WriteLine($"{GetLogPrefix()} [ERROR] {message}");
    }

    public void LogException(Exception exception) {
        _output.WriteLine($"{GetLogPrefix()} [EXCEPTION] {exception}");
    }
    
    public void LogTrace(string message) {
        _output.WriteLine($"{GetLogPrefix()} [TRACE] {message}");
    }

    public void BeginSample(string name) {
        
    }

    public void EndSample() {
        
    }
}