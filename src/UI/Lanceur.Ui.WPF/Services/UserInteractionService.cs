using Lanceur.Core.Services;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Services;

public class UserInteractionService : IUiUserInteractionService
{
    #region Methods

    public async Task<bool> AskAsync(string title, string message, string yes = "Yes", string no = "No")
    {
        var messageBox = new MessageBox { Title = title, Content = message, PrimaryButtonText = yes, CloseButtonText = no };
        var result = await messageBox.ShowDialogAsync();
        return result == MessageBoxResult.Primary;
    }

    public async Task ShowAsync(string title, string message, string ok = "OK", string cancel = "Cancel")
    {
        var messageBox = new MessageBox { Title = title, Content = message, PrimaryButtonText = ok, CloseButtonText = cancel };
        await messageBox.ShowDialogAsync();
    }

    #endregion
}