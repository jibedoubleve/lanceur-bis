using System.Diagnostics;
using Newtonsoft.Json;

namespace Lanceur.SharedKernel.Logging;

public class TraceLoggerScope : IDisposable
{
    #region Fields

    private readonly object _state;

    #endregion

    #region Constructors

    public TraceLoggerScope(object state) => _state = state;

    #endregion

    #region Methods

    public void Dispose()
    {
        var json = JsonConvert.SerializeObject(_state, Formatting.Indented);
        Trace.WriteLine($"Display scope:\r\n{json}");
    }

    #endregion
}