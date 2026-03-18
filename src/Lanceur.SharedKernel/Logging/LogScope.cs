using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Logging;

public sealed class LogScope
{
    #region Fields

    private readonly ILogger _logger;
    private readonly Dictionary<string, object> _scope = new();

    #endregion

    #region Constructors

    public LogScope(ILogger logger) => _logger = logger;

    #endregion

    #region Methods

    public LogScope Add(string key, object value)
    {
        _scope.Add(key, value);
        return this;
    }

    public IDisposable BeginScope() => _logger.BeginScope(_scope) ?? new Disposable();

    #endregion

    private sealed class Disposable : IDisposable
    {
        #region Methods

        public void Dispose()
        {
            /* This is a dummy IDisposable item */
        }

        #endregion
    }
}