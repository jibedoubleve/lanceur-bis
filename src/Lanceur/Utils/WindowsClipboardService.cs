using Lanceur.Core.Services;
using System.Windows;

namespace Lanceur.Utils;

public class WindowsClipboardService : IClipboardService
{
    #region Methods

    public string GetText() => Clipboard.GetText();

    #endregion Methods
}