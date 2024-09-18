namespace Lanceur.Core.Services;

public interface IUiUserInteractionService
{
    Task<bool> AskAsync(string title, string message, string yes = "Yes", string no = "No");
    Task ShowAsync(string title, string message, string ok = "OK", string cancel = "No");
}