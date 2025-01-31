#pragma warning disable RS1035

namespace SourceGenerators.Utils.Logging {
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.CodeAnalysis;

    public static class Logger {
        private static readonly object @lock = new object();
        private static readonly string rootDir;

        static Logger() {
            rootDir = Path.Combine(Path.GetTempPath(), "MorpehSourceGenerators");
            Directory.CreateDirectory(rootDir);
        }

        [Conditional("MORPEH_SOURCEGEN_LOGGING")]
        public static void Log(string pipelineName, string step, string message) {
            lock (@lock) {
                var logFilePath = Path.Combine(rootDir, $"{pipelineName}.log");
                var timeStamp   = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var threadId    = Environment.CurrentManagedThreadId;
                var logLine     = $"[{timeStamp}][{step}] [{threadId}] {message}";

                try {
                    File.AppendAllText(logFilePath, logLine + Environment.NewLine);
                } catch (Exception) {
                    // skip
                }
            }
        }

        [Conditional("MORPEH_SOURCEGEN_LOGGING")]
        public static void LogException(string pipelineName, string step, Exception exception) {
            lock (@lock) {
                var logFilePath = Path.Combine(rootDir, $"{pipelineName}.log");
                var timeStamp   = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var threadId = Environment.CurrentManagedThreadId;
                var logLine     = $"[{timeStamp}][{step}] [{threadId}] {exception.Message}{Environment.NewLine}{exception.StackTrace}";

                try {
                    File.AppendAllText(logFilePath, logLine + Environment.NewLine);
                } catch (Exception) {
                    // skip
                }
            }
        }
        
        public static IncrementalValuesProvider<T> WithLogging<T>(this IncrementalValuesProvider<T> provider, string pipelineName, string step) {
#if MORPEH_SOURCEGEN_LOGGING
            return provider.Select((item, ct) => {
                ct.ThrowIfCancellationRequested();
                
                Log(pipelineName, step, $"Item: {item}");
                return item;
            });
#else
            return provider;
#endif
        }
    }
}

#pragma warning restore RS1035