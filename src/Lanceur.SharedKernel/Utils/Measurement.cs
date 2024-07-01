using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public sealed class Measurement : IDisposable
{
    #region Fields

    private readonly Action<TimeSpan, string> _log;
    private readonly Stopwatch _stopwatch;

    #endregion Fields

    #region Constructors

    internal Measurement(Action<TimeSpan, string> log)
    {
        _log = log;
        _stopwatch = new();
        _stopwatch.Start();
    }

    #endregion Constructors

    #region Properties

    public static Measurement Empty => new(null);

    #endregion Properties

    #region Methods

    private void Log(string message, bool stopTime)
    {
        if(stopTime) _stopwatch.Stop();
        _log(_stopwatch.Elapsed, message);
    }

    public void Dispose() => Log(string.Empty, stopTime: true);

    public void LogSplitTime(string message = null) => Log(message, stopTime: false);

    #endregion Methods
}