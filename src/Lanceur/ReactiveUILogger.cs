using NLog;
using System;
using System.ComponentModel;
using System.Globalization;
using ILogger = Splat.ILogger;
using LogLevel = Splat.LogLevel;
using NLogLevel = NLog.LogLevel;


namespace Lanceur
{
    internal class ReactiveUILogger : ILogger
    {
        #region Fields

        private readonly Logger _logger;

        #endregion Fields

        #region Constructors

        public ReactiveUILogger()
        {
            _logger = LogManager.GetLogger("ReactiveUI");
        }

        #endregion Constructors

        #region Properties

        public LogLevel Level { get; set; }

        #endregion Properties

        #region Methods

        private NLogLevel GetLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Debug => NLogLevel.Trace,
                LogLevel.Info => NLogLevel.Debug,
                LogLevel.Warn => NLogLevel.Warn,
                LogLevel.Error => NLogLevel.Error,
                LogLevel.Fatal => NLogLevel.Fatal,
                _ => throw new NotSupportedException($"Log level '{logLevel}' not supported."),
            };
        }

        public void Write([Localizable(false)] string message, LogLevel logLevel)
        {
            _logger.Log(GetLevel(logLevel), message);
        }

        public void Write(Exception exception, [Localizable(false)] string message, LogLevel logLevel)
        {
            _logger.Log(GetLevel(logLevel), exception, CultureInfo.InvariantCulture, message);
        }

        public void Write([Localizable(false)] string message, [Localizable(false)] Type type, LogLevel logLevel)
        {
            _logger.Log(GetLevel(logLevel), message);
        }

        public void Write(Exception exception, [Localizable(false)] string message, [Localizable(false)] Type type, LogLevel logLevel)
        {
            _logger.Log(GetLevel(logLevel), exception, CultureInfo.InvariantCulture, message);
        }

        #endregion Methods
    }
}