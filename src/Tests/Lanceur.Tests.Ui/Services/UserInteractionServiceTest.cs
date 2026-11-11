using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.WPF.Services;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Ui.Services;

public sealed class UserInteractionServiceTest : IDisposable
{
    #region Methods

    public void Dispose() => WeakReferenceMessenger.Default.Unregister<QuestionRequestMessage>(this);

    [WpfFact]
    public async Task When_data_context_is_omitted_Then_content_data_context_is_used()
    {
        // ARRANGE
        const string viewModel = "undeux";
        var sut = new UserDialogueService();
        var userControl = new UserControl { DataContext = viewModel };
        WeakReferenceMessenger.Default.Register<UserInteractionServiceTest, QuestionRequestMessage>(
            this,
            (_, m) => m.Reply(true)
        );
        // ACT
        var response = await sut.InteractAsync(userControl);

        // ASSERT
        response.DataContext.ShouldBe(viewModel);
    }

    [WpfFact]
    public async Task When_data_context_is_specified_Then_it_is_used()
    {
        // ARRANGE
        const string viewModel = "undeux";
        var sut = new UserDialogueService();
        var userControl = new UserControl();
        WeakReferenceMessenger.Default.Register<UserInteractionServiceTest, QuestionRequestMessage>(
            this,
            (_, m) => m.Reply(true)
        );
        // ACT
        var response = await sut.InteractAsync(userControl, dataContext: viewModel);

        // ASSERT
        response.DataContext.ShouldBe(viewModel);
    }

    #endregion
}