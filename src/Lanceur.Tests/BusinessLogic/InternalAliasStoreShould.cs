﻿using FluentAssertions;
using FluentAssertions.Numeric;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Utils;
using Lanceur.Tests.Utils.ReservedAliases;
using Lanceur.Views;
using NLog.LayoutRenderers;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class InternalAliasStoreShould
    {
        #region Methods

        private static ReservedAliasStore GetStore(IDataService dataService, Type type = null)
        {
            type ??= typeof(NotExecutableTestAlias);
            var asm = Assembly.GetAssembly(type);

            var store = new ReservedAliasStore(asm, dataService);
            return store;
        }

        [Fact]
        public void ReturnCountOfReservedKeywords()
        {
            var store = GetStore(Substitute.For<IDataService>());
            store.ReservedAliases.Should().HaveCount(1); ;
        }

        [Fact]
        public void ReturnExpectedCountOfAliasesFromLanceur()
        {
            var store = GetStore(Substitute.For<IDataService>(), type: typeof(MainViewModel));
            store.ReservedAliases.Should().HaveCount(8);
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
            var ds = Substitute.For<IDataService>();
            ds.RefreshUsage(Arg.Any<IEnumerable<QueryResult>>())
              .ReturnsForAnyArgs(x => x.Args()[0] as IEnumerable<QueryResult>);

            var store = GetStore(ds, type: typeof(MainViewModel));
            var query = new Cmdline(criterion);

            store.Search(query).Should().HaveCount(1);
        }

        [Fact]
        public void ReturnSpecifiedReservedKeyword()
        {
            var ds = Substitute.For<IDataService>();
            ds.RefreshUsage(Arg.Any<IEnumerable<QueryResult>>())
              .Returns(new List<QueryResult>() { new ExecutableTestAlias() });

            var store = GetStore(dataService: ds);
            var query = new Cmdline("anothertest");

            store.Search(query).Should().HaveCount(1); ;
        }

        #endregion Methods
    }
}