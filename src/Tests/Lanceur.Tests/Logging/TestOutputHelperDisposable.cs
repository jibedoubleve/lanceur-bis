using Newtonsoft.Json;

namespace Lanceur.Tests.Logging;

public class TestOutputHelperDisposable : IDisposable
{
    #region Fields

    private readonly object _state;
    private readonly Action<string> _write;

    #endregion Fields

    #region Constructors

    public TestOutputHelperDisposable(object state, Action<string> write)
    {
        _state = state;
        _write = write;
    }

    #endregion Constructors

    #region Methods

    public void Dispose()
    {
        var json = JsonConvert.SerializeObject(_state, Formatting.Indented);
        _write($"[Scope] Dumped scope:\r\n{json}");
    }

    #endregion Methods
}