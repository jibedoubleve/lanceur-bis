using System.Windows;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.Services;

public class MemoryStorageService : IMemoryStorageService
{
    #region Methods

    public string RetrieveText() => Clipboard.GetText();
    public void SaveText(string text) => Clipboard.SetText(text);

    #endregion
}