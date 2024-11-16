using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Logging;

public class LogScope
{
    #region Fields

    private readonly ILogger _logger;
    private readonly Dictionary<string, object> _scope = new();

    #endregion Fields

    #region Constructors

    public LogScope(ILogger logger) => _logger = logger;

    #endregion Constructors

    #region Methods

    public LogScope Add(string key, object value)
    {
        _scope.Add(key, value);
        return this;
    }

    public LogScope AddDestructured(string key, object value)
    {
        if (!key.StartsWith('@')) key = $"@{key}";
        return Add(key, value);
    }

    public IDisposable BeginScope() => _logger.BeginScope(_scope);

    #endregion Methods
}