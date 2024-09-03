using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling.Logging;

public class TestOutputHelperDecoratorForMicrosoftLogging<T> : BaseTestOutputHelperDecorator, ILogger<T>
{
    #region Fields

    private readonly TestOutputHelperDecoratorForMicrosoftLogging _logger;

    #endregion

    #region Constructors

    public TestOutputHelperDecoratorForMicrosoftLogging(ITestOutputHelper output) : base(output) => _logger = new(output);

    #endregion

    #region Methods

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new TestOutputHelperDisposable(state, Write);
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => _logger.Log(logLevel, eventId, state, exception, formatter);

    #endregion
}

public class TestOutputHelperDecoratorForMicrosoftLogging : BaseTestOutputHelperDecorator, ILogger
{
    #region Constructors

    public TestOutputHelperDecoratorForMicrosoftLogging(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    private static string ToShortLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace       => "Trace",
            LogLevel.Debug       => "Debug",
            LogLevel.Information => "Info ",
            LogLevel.Warning     => "Warn ",
            LogLevel.Error       => "Error",
            LogLevel.Critical    => "Fatal",
            LogLevel.None        => "     ",
            _                    => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new TestOutputHelperDisposable(state, Write);

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        var parameters = new object[] { eventId, ToShortLevel(logLevel), message };
        Write(exception, "[{1}] {2}", parameters);
    }

    #endregion
}