using ModernWpf.Controls;
using System.Threading.Tasks;

namespace Lanceur.Ui;

public static class DialogMixin
{
    #region Methods

    public static bool ToBool(this ContentDialogResult result)
    {
        return result switch
        {
            ContentDialogResult.Primary   => true,
            ContentDialogResult.None      => false,
            ContentDialogResult.Secondary => false,
            _                             => false
        };
    }

    #endregion Methods
}

public static class Dialogs
{
    #region Methods

    public static async Task<ContentDialogResult> YesNoQuestion(string question, string title = null)
    {
        var dialog = new ContentDialog { Title = title ?? "Question", Content = question, PrimaryButtonText = "Yes", SecondaryButtonText = "No" };
        return await dialog.ShowAsync();
    }

    #endregion Methods
}