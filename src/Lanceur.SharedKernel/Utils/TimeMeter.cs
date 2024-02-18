using System.Runtime.CompilerServices;

namespace Lanceur.SharedKernel.Utils;

public static class TimeMeter
{
    #region Methods

    public static Measurement Measure(object source,
                                      Action<string, object[]> log,
                                      [CallerMemberName] string callerMemberName = "")
        => new(source.GetType(), callerMemberName, log);

    #endregion Methods
}