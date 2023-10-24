using Splat;
using System.ComponentModel;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public class TestReactiveUiLogger : BaseLogger, ILogger
    {
        #region Constructors

        public TestReactiveUiLogger(ITestOutputHelper output) : base(output)
        {
        }

        #endregion Constructors

        #region Properties

        public LogLevel Level => throw new NotImplementedException();

        #endregion Properties

        #region Methods

        private string Wrap(string message) => $" -- ReactiveUI -- {message, -6}";

        public void Write([Localizable(false)] string message, LogLevel logLevel) => Write(Wrap(message), $"{logLevel}");

        public void Write(Exception exception, [Localizable(false)] string message, LogLevel logLevel) => Write(Wrap(message), $"{logLevel}");

        public void Write([Localizable(false)] string message, [Localizable(false)] Type type, LogLevel logLevel) => Write(Wrap(message), $"{logLevel}");

        public void Write(Exception exception, [Localizable(false)] string message, [Localizable(false)] Type type, LogLevel logLevel) => Write(Wrap(message), exception, $"{logLevel}");

        #endregion Methods
    }
}