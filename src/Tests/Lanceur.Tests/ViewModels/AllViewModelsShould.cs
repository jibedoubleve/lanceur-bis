using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Ui;
using Lanceur.Views;
using ReactiveUI;
using System.Globalization;
using System.Reflection;
using Lanceur.Tests.Tooling;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public class AllViewModelsShould
{
    static AllViewModelsShould()
    {
        var bootstrapper = new Bootstrapper();
        bootstrapper.Invoke("RegisterServices");
        bootstrapper.Invoke("RegisterViewModels");
    }

    #region Methods

    [Theory, InlineData(typeof(AppSettingsViewModel)), InlineData(typeof(DoubloonsViewModel)), InlineData(typeof(HistoryViewModel)), InlineData(typeof(InvalidAliasViewModel)), InlineData(typeof(KeywordsViewModel)), InlineData(typeof(MainViewModel)), InlineData(typeof(MostUsedViewModel)), InlineData(typeof(PluginsViewModel)), InlineData(typeof(SettingsViewModel)), InlineData(typeof(TrendsViewModel))]
    public void InstantiateWithoutError(Type vmType)
    {
        var viewModel = vmType.GetConstructor(Type.EmptyTypes) == null
            ? Activator.CreateInstance(
                vmType,
                BindingFlags.CreateInstance |
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.OptionalParamBinding,
                null,
                new[] { Type.Missing },
                CultureInfo.CurrentCulture
            )
            : Activator.CreateInstance(vmType);

        var fields = from t in vmType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)
                     where t.IsInitOnly && t.IsPrivate
                     select t;

        // Check whether all private readonly fields have a value...
        using (new AssertionScope())
        {
            foreach (var field in fields)
                if (field.FieldType.BaseType != typeof(ReactiveObject) && field.FieldType.BaseType != typeof(RoutableViewModel))
                    field.GetValue(viewModel).Should().NotBeNull($"'{vmType.Name}.{field.Name}' is not expected to be NULL.");
        }
    }
}

#endregion Methods