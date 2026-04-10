using Lanceur.Core.Services;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Tests.Tools.Stubs;

public static class MockWpfServicesExtension
{
    #region Methods

    /// <summary>
    ///     Mocks the UI services that are normally registered by <c>AddWpfServices</c>.
    /// </summary>
    public static IServiceCollection AddUiServicesMockContext(this IServiceCollection serviceCollection)
        => serviceCollection.AddMockSingleton<IUserGlobalNotificationService>()
                            .AddMockSingleton<IUserDialogueService>()
                            .AddMockSingleton<IUserNotificationService>()
                            .AddMockSingleton<IUserCommunicationService>()
                            .AddMockSingleton<IViewFactory>()
                            .AddMockSingleton<IAppRestartService>();

    #endregion
}