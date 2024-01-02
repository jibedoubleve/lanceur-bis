using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public sealed class Measurement : IDisposable
{
    #region Fields

    private readonly string _callerMemberName;
    private readonly Action<string, object[]> _log;
    private readonly Type _source;
    private readonly Stopwatch _stopwatch;

    #endregion Fields

    #region Constructors

    internal Measurement(Type source, string callerMemberName, Action<string, object[]> log)
    {
        _source = source;
        _callerMemberName = callerMemberName;
        _log = log;
        _stopwatch = new();
        _stopwatch.Start();
    }

    #endregion Constructors

    #region Methods

    public void Dispose()
    {
        var elapsed = _stopwatch.ElapsedMilliseconds;
        _stopwatch.Stop();
        var message = "{SourceFullName}.{CallerMemberName} executed in {ElapsedMilliseconds} milliseconds";
        var parameters = new object[] { _source.FullName, _callerMemberName, elapsed };
        _log(message, parameters);
    }

    #endregion Methods
}