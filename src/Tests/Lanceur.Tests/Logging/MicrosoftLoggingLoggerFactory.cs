using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public class MicrosoftLoggingLoggerFactory : ILoggerFactory
    {
        #region Fields

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        public MicrosoftLoggingLoggerFactory(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        public void AddProvider(ILoggerProvider provider)
        { }

        public ILogger CreateLogger(string categoryName) => new TestOutputHelperDecoratorForMicrosoftLogging(_output);

        public void Dispose()
        { }

        #endregion Methods
    }
}