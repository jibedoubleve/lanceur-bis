using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using NSubstitute;

namespace Lanceur.Tests.Utils
{
    internal static class ServiceFactory
    {
        #region Properties

        public static ICmdlineProcessor CmdLineService => Substitute.For<ICmdlineProcessor>();
        public static ILogService LogService => Substitute.For<ILogService>();
        public static ISearchService SearchService => Substitute.For<ISearchService>();

        #endregion Properties
    }
}