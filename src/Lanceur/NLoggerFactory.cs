using Lanceur.Core.Services;
using System;

namespace Lanceur
{
    internal class NLoggerFactory : IAppLoggerFactory
    {
        #region Methods

        public IAppLogger GetLogger<TCategory>() => GetLogger(typeof(TCategory));

        public IAppLogger GetLogger(Type categoryName) => new NLogger(categoryName);

        #endregion Methods
    }
}