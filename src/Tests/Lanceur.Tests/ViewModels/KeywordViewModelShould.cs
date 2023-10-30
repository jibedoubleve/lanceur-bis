using System.Reactive.Disposables;
using System.Windows.Forms.Design;
using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Tests.Utils.Builders;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class KeywordViewModelShould
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion Fields

    #region Constructors

    public KeywordViewModelShould(ITestOutputHelper output)
    {
        _output = output;
    }

    #endregion Constructors

    #region Methods

    [Fact]
    public void AbleToAddMultipleTimesAlias()
    {
        new TestScheduler().With(scheduler =>
        {
            // ARRANGE
            var dbRepository = Substitute.For<IDbRepository>();
            dbRepository.CheckNamesExist(Arg.Any<string[]>())
                        .Returns(new ExistingNameResponse(Array.Empty<string>()));

            var packageValidator = Substitute.For<IPackagedAppValidator>();
            
            var vm = new KeywordsViewModelBuilder()
                     .With(scheduler)
                     .With(_output)
                     .With(dbRepository)
                     .With(packageValidator)
                     .Build();

            var synonyms = Guid.NewGuid().ToString(); 
            var fileName = Guid.NewGuid().ToString();
            
            // ACT
            
            vm.Activate(new());
            scheduler.Start();
            vm.CreatingAlias.Execute().Subscribe();
            var hash = vm.SelectedAlias.GetHashCode();
            
            vm.SelectedAlias.Synonyms = synonyms;
            vm.SelectedAlias.FileName = fileName;
            
            packageValidator.FixAsync(Arg.Any<AliasQueryResult>())
                            .Returns(vm.SelectedAlias);

            vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();
            
            // ASSERT
            vm.SelectedAlias.GetHashCode().Should().Be(hash);

        });
    }

    #endregion Methods
}