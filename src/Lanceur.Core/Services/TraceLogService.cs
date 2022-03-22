using System.Diagnostics;
using _Trace = System.Diagnostics.Trace;

namespace Lanceur.Core.Services
{
    /// <summary>
    /// The use of this logger is NOT recommended. It's there
    /// as a fallback if dev forgot to configure a logger.
    /// This LogService is a very basic and NOT optimised logger.
    /// It uses <see cref="System.Diagnostics.Trace"/> and also
    /// reflection to log the name of the calling method.
    /// </summary>
    public class TraceLogService : ILogService
    {
        #region Methods

        private static string GetCallerName()
        {
            var stackTrace = new StackTrace();
            var method = stackTrace.GetFrame(1).GetMethod();
            return method.Name;
        }

        private static void Write(string message, Exception ex = null)
        {
            var name = GetCallerName();

            _Trace.WriteLine($"[{name}] {message}{Environment.NewLine}{(ex is not null ? ex : string.Empty)}");
        }

        private static void Write(Exception ex)
        {
            var name = GetCallerName();

            _Trace.WriteLine($"[{name}] {ex}");
        }

        public void Debug(string message) => Write(message);

        public void Debug(Exception ex) => Write(ex);

        public void Error(string message, Exception ex = null) => Write(message);

        public void Fatal(string message, Exception ex = null) => Write(message);

        public void Info(string message) => Write(message);

        public void Trace(string message) => Write(message);

        public void Warning(string message, Exception ex = null) => Write(message);

        public void Warning(Exception ex) => Write(ex);

        #endregion Methods
    }
}