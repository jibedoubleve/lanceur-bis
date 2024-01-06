using System.Diagnostics;
using System.Runtime.CompilerServices;

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

    public static Measurement Empty => new(typeof(object), "", null);

    #endregion Constructors

    #region Methods

    public void Dispose()
    {
        var elapsed = _stopwatch.ElapsedMilliseconds;
        _stopwatch.Stop();
        const string message = "Execution of {SourceFullName}.{CallerMemberName} in {ElapsedMilliseconds} milliseconds";
        var parameters = new object[] { _source.FullName, _callerMemberName, elapsed };
        _log(message, parameters);
    }

    #endregion Methods
}