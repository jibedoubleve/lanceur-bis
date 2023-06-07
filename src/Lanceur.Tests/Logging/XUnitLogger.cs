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

        public void Debug(string message) => Write(message);

        public void Debug(Exception ex) => Write(ex);

        public void Error(string message, Exception ex = null) => Write(message, ex);

        public void Fatal(string message, Exception ex = null) => Write(message);

        public void Info(string message) => Write(message);

        public void Trace(string message) => Write(message);

        public void Warning(string message, Exception ex = null) => Write(message);

        public void Warning(Exception ex) => Write(ex);

        #endregion Methods
    }
}