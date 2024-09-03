using System.Windows;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Win32.Services;

public class WindowsClipboardService : IClipboardService
{
    #region Methods

    public string GetText() => Clipboard.GetText();

    #endregion Methods
}