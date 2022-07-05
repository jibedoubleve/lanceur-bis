using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Utils.ReservedAliases;
using Lanceur.Views;
using System.Reflection;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class InternalAliasStoreShould
    {
        #region Methods

        private static ReservedAliasStore GetStore(Type type = null)
        {
            type ??= typeof(NotExecutableTestAlias);
            var asm = Assembly.GetAssembly(type);
            var store = new ReservedAliasStore(asm);
            return store;
        }

        [Fact]
        public void ReturnCountOfReservedKeywords()
        {
            var store = GetStore();
            store.ReservedAliases.Should().HaveCount(1); ;
        }

        [Fact]
        public void ReturnExpectedCountOfAliasesFromLanceur()
        {
            var store = GetStore(typeof(MainViewModel));
            store.ReservedAliases.Should().HaveCount(7);
        }

        [Theory]
        [InlineData("add")]
        [InlineData("import")]
        [InlineData("quit")]
        [InlineData("sessions")]
        [InlineData("setup")]
        [InlineData("version")]
        public void ReturnSpecifiedReservedAliasFromLanceur(string criterion)
        {
            var store = GetStore(typeof(MainViewModel));
            var query = new Cmdline(criterion);
            store.Search(query).Should().HaveCount(1);
        }

        [Theory]
        [InlineData("anothertest")]
        public void ReturnSpecifiedReservedKeyword(string criterion)
        {
            var store = GetStore();
            var query = new Cmdline(criterion);

            store.Search(query).Should().HaveCount(1); ;
        }

        #endregion Methods
    }
}