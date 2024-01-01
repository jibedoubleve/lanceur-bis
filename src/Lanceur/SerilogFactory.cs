using Lanceur.Core.Services;
using System;

namespace Lanceur
{
    internal class SerilogFactory : IAppLoggerFactory
    {
        #region Methods

        public IAppLogger GetLogger<TSourceContext>() => new SerilogLogger(typeof(TSourceContext));

        public IAppLogger GetLogger(Type sourceContext) => new SerilogLogger(sourceContext);

        #endregion Methods
    }
}