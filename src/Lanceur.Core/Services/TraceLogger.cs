using System.Diagnostics;
using Lanceur.SharedKernel.Mixins;
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
    public class TraceLogger : IAppLogger
    {
        #region Methods

        private static string GetCallerName()
        {
            var stackTrace = new StackTrace();
            var method = stackTrace!.GetFrame(1)!.GetMethod();
            return method!.Name;
        }

        private static void Write(Exception ex, string message, params object[] parameterValues)
        {
            var name = GetCallerName();

            _Trace.WriteLine($"[{name}] {message.Format(parameterValues)}{Environment.NewLine}{(ex is not null ? ex : string.Empty)}");
        }

        private static void Write(Exception ex)
        {
            var name = GetCallerName();

            _Trace.WriteLine($"[{name}] {ex}");
        }

        public void Debug(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Debug(Exception ex) => Write(ex);

        public void Error(Exception ex, string message, params object[] propertyValues) => Write(ex, message, propertyValues);

        public void Fatal(Exception ex, string message, params object[] propertyValues) => Write(ex, message, propertyValues);

        public void Info(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Trace(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Warning(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Warning(Exception ex, string message, params object[] propertyValues) => Write(ex, message, propertyValues);

        public void Warning(Exception ex) => Write(ex);

        #endregion Methods
    }
}