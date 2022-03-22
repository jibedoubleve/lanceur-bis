using FluentAssertions;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models
{
    public class CmdlineShould
    {
        #region Methods

        [Fact]
        public void HaveEmptyNameByDefault()
        {
            Cmdline.Empty.Name.Should().BeEmpty();
        }

        [Fact]
        public void HaveEmptyNameWhenCtorNull()
        {
            new Cmdline(null, null).Name.Should().BeEmpty();
        }

        [Fact]
        public void HaveEmptyParametersByDefault()
        {
            Cmdline.Empty.Parameters.Should().BeEmpty();
        }

        [Fact]
        public void HaveEmptyParametersWhenCtorNull()
        {
            new Cmdline(null, null).Parameters.Should().BeEmpty();
        }

        #endregion Methods
    }
}