using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Constants;
using Lanceur.Ui.Core.Messages;
using ScottPlot.Interactivity;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace Lanceur.Ui.WPF.Services;

public class UserInteractionService : IUserInteractionService
{
    #region Methods

    ///<inheritdoc />
    public async Task<bool> AskAsync(string message, string title = "Question", string yes = ButtonLabels.Yes, string no = ButtonLabels.No)
    {
        var messageBox = new MessageBox { Title = title, Content = message, PrimaryButtonText = yes, CloseButtonText = no };
        var result = await messageBox.ShowDialogAsync();
        return result == MessageBoxResult.Primary;
    }

    ///<inheritdoc />
    public async Task<bool> AskUserYesNoAsync(object content, string yesText = "Yes", string noText = "No", string title = "Question")
    {
        return await WeakReferenceMessenger.Default.Send<QuestionRequestMessage>(
            new(
                content,
                title,
                yesText,
                noText
            )
        );
    }

    ///<inheritdoc />
    public async Task<(bool IsConfirmed, object DataContext)> InteractAsync(object content, string yesText = ButtonLabels.Apply, string noText = ButtonLabels.Cancel, string title = "Interaction",  object? dataContext = null)
    {
        if (content is FrameworkElement d)
        {
            if(dataContext is not null)
                d.DataContext = dataContext;
            else dataContext = d.DataContext;
        }
        var isConfirmed = await WeakReferenceMessenger.Default.Send<QuestionRequestMessage>(
            new(content, title, yesText, noText)
        );
        return (IsConfirmed: isConfirmed, DataContext: dataContext)!;
    }

    ///<inheritdoc />
    public async Task ShowAsync(string title, object content, string ok = ButtonLabels.Close, string? cancel = null)
    {
        MessageBox messageBox = cancel is null
            ? new() { Title = title, Content = content }
            : new() { Title = title, Content = content, PrimaryButtonText = ok };
        await messageBox.ShowDialogAsync();
    }

    #endregion
}