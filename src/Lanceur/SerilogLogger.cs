using Lanceur.Core.Services;
using Serilog;
using System;

namespace Lanceur
{
    internal class SerilogLogger : IAppLogger
    {
        private readonly ILogger _logger;

        public SerilogLogger(Type sourceContext)
        {
            _logger = Log.Logger.ForContext(sourceContext);
        }
        #region Methods

        public void Debug(string message, params object[] propertyValues) => _logger.Debug(message, propertyValues);

        public void Debug(Exception ex) => _logger.Debug(ex, "An error occured: {message}", ex.Message);

        public void Error(Exception ex, string message, params object[] propertyValues) => _logger.Error(ex, message, propertyValues);

        public void Fatal(Exception ex, string message, params object[] propertyValues) => _logger.Fatal(ex, message, propertyValues);

        public void Info(string message, params object[] propertyValues) => _logger.Information(message, propertyValues);

        public void Trace(string message, params object[] propertyValues) => _logger.Verbose(message, propertyValues);

        public void Warning(string message, params object[] propertyValues) => _logger.Warning(message, propertyValues);
        
        public void Warning(Exception ex, string message, params object[] propertyValues) => _logger.Warning(ex, message, propertyValues);

        public void Warning(Exception ex) => Log.Logger.Warning(ex,"A warning: {message}", ex.Message);

        #endregion Methods
    }
}