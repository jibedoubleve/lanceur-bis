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

        [Theory]
        [InlineData("ini", "thb@joplin@spotify")]
        public async Task ExecuteMultiMacro(string cmd, string parameters)
        {
            var cmdline = new Cmdline(cmd, parameters);
            var cmdlineManager = Substitute.For<ICmdlineManager>();
            cmdlineManager
                .BuildFromText((string)cmdline)
                .Returns(cmdline);

            var mgr = new ExecutionManager(
                Substitute.For<IAppLoggerFactory>(),
                Substitute.For<IWildcardManager>(),
                Substitute.For<IDataService>(),
                cmdlineManager
            );

            var macro = Substitute.For<MultiMacro>();
            await macro.ExecuteAsync(
                Arg.Do<Cmdline>(
                    x => x.Should().NotBeNull()
                )
            );

            var request = new ExecutionRequest
            {
                Query = (string)cmdline,
                ExecuteWithPrivilege = false,
                QueryResult = macro
            };

            await mgr.ExecuteAsync(request);
        }

        #endregion Methods
    }
}