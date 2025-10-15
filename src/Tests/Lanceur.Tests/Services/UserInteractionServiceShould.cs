using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Shouldly;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.WPF.Services;
using Xunit;

namespace Lanceur.Tests.Services;

public class UserInteractionServiceShould : IDisposable
{
    #region Methods

    public void Dispose() => WeakReferenceMessenger.Default.Unregister<QuestionRequestMessage>(this);

    [WpfFact]
    public async Task ReturnContentDataContextWhenDataContextOmmited()
    {
        // ARRANGE
        const string viewModel = "undeux";
        var sut = new UserInteractionService();
        var userControl = new UserControl { DataContext = viewModel };
        WeakReferenceMessenger.Default.Register<UserInteractionServiceShould, QuestionRequestMessage>(
            this,
            (_, m) => m.Reply(true)
        );
        // ACT
        var response = await sut.InteractAsync(userControl);

        // ASSERT
        response.DataContext.Should().Be(viewModel);
    }

    [WpfFact]
    public async Task UseSpecifiedDataContextWithPriority()
    {
        // ARRANGE
        const string viewModel = "undeux";
        var sut = new UserInteractionService();
        var userControl = new UserControl();
        WeakReferenceMessenger.Default.Register<UserInteractionServiceShould, QuestionRequestMessage>(
            this,
            (_, m) => m.Reply(true)
        );
        // ACT
        var response = await sut.InteractAsync(userControl, dataContext: viewModel);

        // ASSERT
        response.DataContext.Should().Be(viewModel);
    }

    #endregion
}