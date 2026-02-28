using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.Tools.Logging;

public class MicrosoftLoggingLoggerFactory : ILoggerFactory
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public MicrosoftLoggingLoggerFactory(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => new TestOutputHelperDecoratorForMicrosoftLogging(_output);

    public void Dispose() { }

    #endregion
}