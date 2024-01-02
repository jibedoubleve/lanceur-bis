using Newtonsoft.Json;
using System.Diagnostics;

namespace Lanceur.Infra.Logging;

public class TraceLoggerScope : IDisposable
{
    #region Fields

    private readonly object _state;

    #endregion Fields

    #region Constructors

    public TraceLoggerScope(object state)
    {
        _state = state;
    }

    #endregion Constructors

    #region Methods

    public void Dispose()
    {
        var json = JsonConvert.SerializeObject(_state, Formatting.Indented);
        Trace.WriteLine($"Display scope:\r\n{json}");
    }

    #endregion Methods
}