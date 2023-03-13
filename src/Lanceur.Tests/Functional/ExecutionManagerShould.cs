using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Macros;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class ExecutionManagerShould
    {
        #region Methods

        [Fact]
        public async Task ExecuteMultiMacro()
        {
            var mgr = new ExecutionManager(
                Substitute.For<IAppLoggerFactory>(),
                Substitute.For<IWildcardManager>(),
                Substitute.For<IDataService>()
            );

            var macro = Substitute.For<MultiMacro>();
            await macro.ExecuteAsync(
                Arg.Do<Cmdline>(
                    x => x.Should().NotBeNull()
                )
            );

            var request = new ExecutionRequest
            {
                Cmdline = new Cmdline("ini", "thb@joplin@spotify"),
                ExecuteWithPrivilege = false,
                QueryResult = macro
            };

            await mgr.ExecuteAsync(request);
        }

        #endregion Methods
    }
}