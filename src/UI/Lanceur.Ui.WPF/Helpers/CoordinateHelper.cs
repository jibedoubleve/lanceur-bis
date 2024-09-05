using System.Windows;
using Lanceur.Core.Models.Settings;

namespace Lanceur.Ui.WPF.Helpers;

public static class CoordinateHelper
{
    public static bool IsAtPosition(this Window window, PositionSection position)
    {
        ArgumentNullException.ThrowIfNull(nameof(position));
        ArgumentNullException.ThrowIfNull(nameof(window));
        
        return position.Top == window.Top &&
               position.Left == window.Left;
    }
}