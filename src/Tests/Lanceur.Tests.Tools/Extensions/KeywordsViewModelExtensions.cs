using Lanceur.Tests.Tools.Helpers;
using Lanceur.Ui.Core.ViewModels.Pages;
using Shouldly;

namespace Lanceur.Tests.Tools.Extensions;

public static class KeywordsViewModelExtensions
{
    #region Methods

    public static async Task CreateNewAlias(
        this KeywordsViewModel viewModel,
        string name,
        string? fileName = null,
        string? luaScript = null
    )
    {
        viewModel.PrepareAliasForCreation(name, fileName, luaScript);
        await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null); // Save the changes
    }

    /// <summary>
    ///     Prepares the view model for alias creation by triggering the create command
    ///     and setting the alias properties without saving.
    /// </summary>
    public static void PrepareAliasForCreation(
        this KeywordsViewModel viewModel,
        string name,
        string? fileName = null,
        string? luaScript = null
    )
    {
        fileName ??= Any.String(10);

        // --- Create a new alias
        viewModel.CreateAliasCommand.Execute(null); // Prepare alias for creation

        viewModel.SelectedAlias.ShouldNotBeNull("it is just setup for creation");
        viewModel.SelectedAlias!.Synonyms = name;
        viewModel.SelectedAlias!.FileName = fileName;
        viewModel.SelectedAlias!.LuaScript = luaScript;
    }

    #endregion
}