using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra;
using Lanceur.Infra.Services;
using Splat;
using System;

namespace Lanceur.Utils
{
    internal static class LogService
    {
        #region Properties

        public static ILogService Current
        {
            get => Locator.Current.GetService<ILogService>() ?? new TraceLogService();
        }

        #endregion Properties

        #region Methods

        public static ILogService Log(this QueryResult _)
        {
            return Current;
        }

        #endregion Methods
    }
}