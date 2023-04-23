using Lanceur.Core.Services;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public class XUnitLoggerFactory : IAppLoggerFactory
    {
        #region Fields

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        public XUnitLoggerFactory(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        public IAppLogger GetLogger<TSource>() => new XUnitLogger(_output);

        public IAppLogger GetLogger(Type sourceType) => new XUnitLogger(_output);

        #endregion Methods
    }
}