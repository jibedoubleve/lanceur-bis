using FluentAssertions;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Tests.ViewModels.Helpers;

public static class ViewModelExtensions
{
    public static async Task CreateNewAlias(this KeywordsViewModel viewModel, string name, string fileName)
    {
        // --- Create a new alias
        viewModel.CreateAliasCommand.Execute(null); // Prepare alias for creation

        viewModel.SelectedAlias.Should().NotBeNull("it is just setup for creation");
        viewModel.SelectedAlias!.Synonyms = name;
        viewModel.SelectedAlias!.FileName = fileName;

        await viewModel.SaveCurrentAliasCommand.ExecuteAsync(null); // Save the changes
    }
}