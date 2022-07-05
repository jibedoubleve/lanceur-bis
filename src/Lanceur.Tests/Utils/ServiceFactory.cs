using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using NSubstitute;

namespace Lanceur.Tests.Utils
{
    internal static class ServiceFactory
    {
        #region Properties

        public static ICmdlineManager CmdLineService => Substitute.For<ICmdlineManager>();
        public static ILogService LogService => Substitute.For<ILogService>();
        public static IMacroManager MacroManager => Substitute.For<IMacroManager>();
        public static ISearchService SearchService => Substitute.For<ISearchService>();

        #endregion Properties
    }
}