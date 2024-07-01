namespace Lanceur.SharedKernel.Utils;

public static class TimeMeter
{
    #region Methods

    public static Measurement Measure(Action<TimeSpan, string> log) => new(log);

    #endregion Methods
}