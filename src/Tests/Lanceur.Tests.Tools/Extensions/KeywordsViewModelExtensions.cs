using Shouldly;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Tests.Tools.Extensions;

public static class KeywordsViewModelExtensions
{
    #region Methods

    public static async Task CreateNewAlias(this KeywordsViewModel viewModel, string name, string? fileName = null, string? luaScript = null)
    {
        viewModel.PrepareAliasForCreation(name, fileName, luaScript);
        await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null); // Save the changes
    }

    public static void PrepareAliasForCreation(this KeywordsViewModel viewModel, string name, string? fileName = null, string? luaScript = null)
    {
        fileName ??= $"{Guid.NewGuid()}";

        // --- Create a new alias
        viewModel.CreateAliasCommand.Execute(null); // Prepare alias for creation

        viewModel.SelectedAlias.ShouldNotBeNull("it is just setup for creation");
        viewModel.SelectedAlias!.Synonyms = name;
        viewModel.SelectedAlias!.FileName = fileName;
        viewModel.SelectedAlias!.LuaScript = luaScript;
    }

    #endregion
}