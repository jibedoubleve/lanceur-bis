using System.Windows;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;

namespace Lanceur.Ui.WPF.Helpers;

public static class CoordinateHelper
{
    #region Methods

    public static bool IsAtPosition(this Window window, PositionSection position)
    {
        ArgumentNullException.ThrowIfNull(nameof(position));
        ArgumentNullException.ThrowIfNull(nameof(window));

        return position.Top == window.Top &&
               position.Left == window.Left;
    }

    public static Coordinate ToCoordinate(this PositionSection position) => new(position.Top, position.Left);
    public static Coordinate ToCoordinate(this Window window) => new(window.Top, window.Left);

    #endregion
}