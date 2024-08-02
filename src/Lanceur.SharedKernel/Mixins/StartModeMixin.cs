using System.Diagnostics;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.SharedKernel.Mixins;

public static class StartModeMixin
{
    #region Methods

    public static ProcessWindowStyle AsWindowsStyle(this StartMode mode)
    {
        return mode switch
        {
            StartMode.Default   => ProcessWindowStyle.Normal,
            StartMode.Maximised => ProcessWindowStyle.Maximized,
            StartMode.Minimised => ProcessWindowStyle.Minimized,
            _                   => throw new NotSupportedException($"The 'StartMode' of '{mode}' is not supported. Did you forget to support it?")
        };
    }

    #endregion Methods
}