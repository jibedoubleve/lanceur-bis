using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public sealed class Measurement : IDisposable
{
    #region Fields

    private readonly Action<TimeSpan> _log;
    private readonly long startTime;

    #endregion

    #region Constructors

    internal Measurement(Action<TimeSpan> log)
    {
        _log = log;
        startTime = Stopwatch.GetTimestamp();
    }

    #endregion

    #region Properties

    public static Measurement Empty => new(null);

    #endregion

    #region Methods

    public void Dispose() => _log?.Invoke(Stopwatch.GetElapsedTime(startTime));

    #endregion
}