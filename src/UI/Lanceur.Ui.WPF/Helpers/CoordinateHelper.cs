using System.Windows;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.Helpers;

public static class CoordinateHelper
{
    #region Methods

    /// <summary>
    ///     Determines whether the window is located at a specific position.
    /// </summary>
    /// <param name="window">The window whose position is being checked.</param>
    /// <param name="position">The <see cref="PositionSection" /> object that contains the position to compare against.</param>
    /// <returns>
    ///     <c>true</c> if the window's position matches the specified position (top and left); otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if either <paramref name="window" /> or <paramref name="position" /> is <c>null</c>.
    /// </exception>
    public static bool IsAtPosition(this Window window, PositionSection position)
    {
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(window);
        const float epsilon = 0.001f; // Handling imprecision of numbers

        return Math.Abs(position.Top - window.Top) < epsilon &&
               Math.Abs(position.Left - window.Left) < epsilon;
    }

    public static Coordinate ToCoordinate(this PositionSection position) => new(position.Left, position.Top);
    public static Coordinate ToCoordinate(this Window window) => new(window.Left, window.Top);

    #endregion
}