using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Managers;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Schedulers;
using Lanceur.Ui;
using Lanceur.Utils;
using Lanceur.Views;
using NSubstitute;
using ReactiveUI;
using Splat;
using System.Globalization;
using System.Reflection;
using Xunit;

namespace Lanceur.Tests.ViewModels
{
    public class AllViewModelsShould
    {
        static AllViewModelsShould()
        {
            var l = Locator.CurrentMutable;
            l.Register(() => Substitute.For<IAppLoggerFactory>());
            l.Register(() => Substitute.For<IDbRepository>());
            l.Register(() => Substitute.For<ISchedulerProvider>());
            l.Register(() => Substitute.For<IUserNotification>());
            l.Register(() => Substitute.For<IThumbnailManager>());
            l.Register(() => Substitute.For<IPackagedAppManager>());
            l.Register(() => Substitute.For<INotification>());
            l.Register(() => Substitute.For<IDelay>());
            l.Register(() => Substitute.For<IAppRestart>());
            l.Register(() => Substitute.For<IPackagedAppValidator>());
            l.Register(() => Substitute.For<ICmdlineManager>());
            l.Register(() => Substitute.For<IExecutionManager>());
            l.Register(() => Substitute.For<IAppConfigRepository>());
            l.Register(() => Substitute.For<IDatabaseConfigRepository>());
            l.Register(() => Substitute.For<ISearchService>());
        }

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
        public void InstanciatedWithoutError(Type vmType)
        {
            var viewModel = (vmType.GetConstructor(Type.EmptyTypes) == null)
                             ? Activator.CreateInstance(vmType,
                                    BindingFlags.CreateInstance |
                                    BindingFlags.Public |
                                    BindingFlags.Instance |
                                    BindingFlags.OptionalParamBinding, null, new object[] { Type.Missing }, CultureInfo.CurrentCulture)
                            : Activator.CreateInstance(vmType);

            var fields = from t in vmType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)
                         where t.IsInitOnly
                            && t.IsPrivate
                         select t;

            // Check whether all private readonly fields have a value...
            using (new AssertionScope())
            {
                foreach (var field in fields)
                {
                    if (field.FieldType.BaseType != typeof(ReactiveObject)
                     && field.FieldType.BaseType != typeof(RoutableViewModel))
                    {
                        field.GetValue(viewModel).Should().NotBeNull($"'{vmType.Name}.{field.Name}' is not expected to be NULL.");
                    }
                }
            }
        }
    }

    #endregion Methods
}