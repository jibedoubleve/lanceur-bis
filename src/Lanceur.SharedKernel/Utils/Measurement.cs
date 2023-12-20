using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public sealed class Measurement : IDisposable
{
    #region Fields

    private readonly string _callerMemberName;
    private readonly Action<string> _log;
    private readonly Type _source;
    private readonly Stopwatch _stopwatch;

    #endregion Fields

    #region Constructors

    internal Measurement(Type source, string callerMemberName, Action<string> log)
    {
        _source           = source;
        _callerMemberName = callerMemberName;
        _log              = log;
        _stopwatch        = new();
        _stopwatch.Start();
    }

    #endregion Constructors

    #region Methods

    public void Dispose()
    {
        _stopwatch.Stop();
        var message = $"'{_source.FullName}.{_callerMemberName}' executed in {_stopwatch.ElapsedMilliseconds} milliseconds.";
        _log(message);
    }

    #endregion Methods
}