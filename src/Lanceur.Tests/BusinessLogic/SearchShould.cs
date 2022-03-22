using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Utils;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class SearchShould
    {
        #region Methods

        private static SearchService BuildSearchService(IStoreLoader loader = null)
        {
            loader ??= new DebugStoreLoader();
            var service = new SearchService(loader);
            return service;
        }

        [Fact]
        public void HaveStores()
        {
            SearchService service = BuildSearchService(new StoreLoader());

            service.Stores.Should().HaveCount(4);
        }

        [Fact]
        public void ReturnValues()
        {
            var service = BuildSearchService();
            var query = new Cmdline("code");

            var result = service.Search(query);

            result.Should().HaveCount(1);
            result.ElementAt(0).IsResult.Should().BeFalse();
        }

        #endregion Methods
    }
}