using Lanceur.Core.Services;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.Views.Pages;

namespace Lanceur.Ui.WPF.Services;

public class NavigationService : INavigationService
{
    #region Fields

    private readonly PageNavigator _pageNavigator;

    #endregion

    #region Constructors

    public NavigationService(IServiceProvider serviceProvider) => _pageNavigator = new(serviceProvider);

    #endregion

    #region Methods

    public void NavigateToAnalytics() { throw new NotImplementedException(); }

    public void NavigateToKeywords() { _pageNavigator.NavigateToSettings<KeywordsView>(); }

    #endregion
}