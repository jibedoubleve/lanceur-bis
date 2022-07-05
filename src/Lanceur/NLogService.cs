using Lanceur.Core.Services;
using NLog;
using System;

namespace Lanceur
{
    internal class NLogService : ILogService
    {
        #region Fields

        private const string DEFAULT_NAME = "Default";
        private readonly Logger _logger;

        #endregion Fields

        #region Constructors

        public NLogService(string loggerName = DEFAULT_NAME)
        {
            _logger = LogManager.GetLogger(loggerName ?? DEFAULT_NAME);
        }

        #endregion Constructors

        #region Methods

        public void Debug(string message) => _logger.Debug(message);

        public void Debug(Exception ex) => _logger.Debug(ex);

        public void Error(string message, Exception ex = null)
        {
            if (ex != null) { _logger.Error(ex, message); }
            else { _logger.Error(message); }
        }

        public void Fatal(string message, Exception ex = null)
        {
            if (ex != null) { _logger.Fatal(ex, message); }
            else { _logger.Fatal(message); }
        }

        public void Info(string message) => _logger.Info(message);

        public void Trace(string message) => _logger.Trace(message);

        public void Warning(string message, Exception ex = null)
        {
            if (ex != null) { _logger.Warn(ex, message); }
            else { _logger.Warn(message); }
        }

        public void Warning(Exception ex) => Warning(ex.Message, ex);

        #endregion Methods
    }
}