using System.Runtime.CompilerServices;

namespace Lanceur.SharedKernel.Utils;

public static class TimePiece
{
    #region Methods

    public static Measurement Measure(object source, 
                                      Action<string> log,
                                      [CallerMemberName] string callerMemberName = "") 
        => new(source.GetType(), callerMemberName, log);

    #endregion Methods
}