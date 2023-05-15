using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Ui;
using Lanceur.Views;
using NSubstitute;
using Splat;
using System.Globalization;
using System.Reflection;
using Xunit;

namespace Lanceur.Tests.ViewModels
{
    public class ViewModelShould
    {
        #region Methods

        [Theory]
        [InlineData(typeof(AppSettingsViewModel))]
        [InlineData(typeof(DoubloonsViewModel))]
        [InlineData(typeof(HistoryViewModel))]
        [InlineData(typeof(InvalidAliasViewModel))]
        [InlineData(typeof(KeywordsViewModel))]
        [InlineData(typeof(MainViewModel))]
        [InlineData(typeof(MostUsedViewModel))]
        [InlineData(typeof(PluginsViewModel))]
        [InlineData(typeof(SessionsViewModel))]
        [InlineData(typeof(SettingsViewModel))]
        [InlineData(typeof(TrendsViewModel))]
        public void BeInstanciatedWithoutError(Type viewModelType)
        {
            var l = Locator.CurrentMutable;
            l.Register(() => Substitute.For<IAppLoggerFactory>());
            l.Register(() => Substitute.For<IDataService>());
            l.Register(() => Substitute.For<ISchedulerProvider>());
            l.Register(() => Substitute.For<IUserNotification>());
            l.Register(() => Substitute.For<IThumbnailManager>());
            l.Register(() => Substitute.For<IPackagedAppManager>());
            l.Register(() => Substitute.For<INotification>());

            var act = () =>
            {
                if (viewModelType.GetConstructor(Type.EmptyTypes) == null)
                {
                    Activator.CreateInstance(viewModelType,
                        BindingFlags.CreateInstance |
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.OptionalParamBinding, null, new object[] { Type.Missing }, CultureInfo.CurrentCulture);
                }
                else { Activator.CreateInstance(viewModelType); }
            };

            act.Should().NotThrow();
        }
    }

    #endregion Methods
}