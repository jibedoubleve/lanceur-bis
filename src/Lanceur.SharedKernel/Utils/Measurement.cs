using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public sealed class Measurement : IDisposable
{
    #region Fields

    private readonly Action<TimeSpan> _log;
    private readonly Stopwatch _stopwatch;

    #endregion

    #region Constructors

    internal Measurement(Action<TimeSpan> log)
    {
        _log = log;
        _stopwatch = new();
        _stopwatch.Start();
    }

    #endregion

    #region Properties

    public static Measurement Empty => new(null);

    #endregion

    #region Methods

    public void Dispose() => _log?.Invoke(_stopwatch.Elapsed);

    #endregion
}