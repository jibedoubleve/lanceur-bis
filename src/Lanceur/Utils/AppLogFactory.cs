using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Splat;
using System;

namespace Lanceur.Utils
{
    internal static class AppLogFactory
    {
        #region Methods

        public static IAppLogger Get<TCategory>() => Locator.Current.GetLogger(typeof(TCategory));

        public static IAppLogger Get(Type category) => Locator.Current.GetLogger(category);

        public static IAppLogger Log(this QueryResult _) => Get<QueryResult>();

        #endregion Methods
    }
}