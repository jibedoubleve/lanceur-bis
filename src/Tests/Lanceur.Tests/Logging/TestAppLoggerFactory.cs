using Lanceur.Core.Services;

namespace Lanceur.Tests.Logging
{
    internal class TestAppLoggerFactory : IAppLoggerFactory
    {
        #region Fields

        private readonly IAppLogger _appLogger = new TraceLogger();

        #endregion Fields

        #region Methods

        public IAppLogger GetLogger<TSource>() => _appLogger;

        public IAppLogger GetLogger(Type sourceContext) => _appLogger;

        #endregion Methods
    }
}