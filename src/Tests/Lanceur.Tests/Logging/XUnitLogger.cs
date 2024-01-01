using Lanceur.Core.Services;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public class XUnitLogger : BaseLogger, IAppLogger
    {
        #region Constructors

        public XUnitLogger(ITestOutputHelper output) : base(output)
        {
        }

        #endregion Constructors

        #region Methods

        public void Debug(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Debug(Exception ex) => Write(ex, ex.Message, Array.Empty<object>());

        public void Error(Exception ex, string message, params object[] propertyValues) =>
            Write(ex, message, propertyValues);

        public void Fatal(Exception ex, string message, params object[] propertyValues) =>
            Write(null, message, propertyValues);

        public void Info(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Trace(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Warning(string message, params object[] propertyValues) => Write(null, message, propertyValues);

        public void Warning(Exception ex, string message, params object[] propertyValues) =>
            Write(ex, message, propertyValues);

        public void Warning(Exception ex) => Write(ex, ex.Message, Array.Empty<object>());

        #endregion Methods
    }
}