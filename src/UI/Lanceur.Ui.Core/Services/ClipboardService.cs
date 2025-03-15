using System.Windows;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.Services;

public class ClipboardService : IClipboardService
{
    #region Methods

    public string RetrieveText() => Clipboard.GetText(TextDataFormat.Text);
    public void SaveText(string text) => Clipboard.SetText(text);

    #endregion
}