using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public sealed class Measurement : IDisposable
{
    #region Fields

    private readonly Action<TimeSpan> _log;
    private readonly Action<TimeSpan> _splitLog;
    private readonly long _startTick;
    private long _lastTick ;
    #endregion

    #region Constructors

    internal Measurement(Action<TimeSpan> log, Action<TimeSpan> splitLog)
    {
        _log = log;
        _splitLog = splitLog;
        _startTick = Stopwatch.GetTimestamp();
        _lastTick = _startTick;
    }

    #endregion

    #region Properties

    public static Measurement Empty => new(null, null);

    #endregion

    #region Methods

    public void Dispose() => _log?.Invoke(Stopwatch.GetElapsedTime(_startTick));

    #endregion

    public void Tick()
    {
        _splitLog?.Invoke(Stopwatch.GetElapsedTime(_lastTick));
        _lastTick = Stopwatch.GetTimestamp();
    }
}