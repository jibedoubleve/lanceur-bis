using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Messages;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace Lanceur.Ui.WPF.Services;

public class UserUserInteractionService : IUserInteractionService
{
    #region Methods

    ///<inheritdoc />
    public async Task<bool> AskAsync(string message, string title, string yes = "Yes", string no = "No")
    {
        var messageBox = new MessageBox { Title = title, Content = message, PrimaryButtonText = yes, CloseButtonText = no };
        var result = await messageBox.ShowDialogAsync();
        return result == MessageBoxResult.Primary;
    }

    ///<inheritdoc />
    public async Task<bool> AskUserYesNoAsync(object content, string yesTextMessage = "yes", string noTextMessage = "no", string title = "Question", object? dataContext = null)
    {
        if (content is FrameworkElement d) d.DataContext = dataContext;
        return await WeakReferenceMessenger.Default.Send<QuestionRequestMessage>(
            new(
                content,
                title,
                yesTextMessage,
                noTextMessage
            )
        );
    }

    ///<inheritdoc />
    public async Task ShowAsync(string title, object content, string ok = "OK", string cancel = "Cancel")
    {
        var messageBox = new MessageBox { Title = title, Content = content, PrimaryButtonText = ok, CloseButtonText = cancel };
        await messageBox.ShowDialogAsync();
    }

    #endregion
}