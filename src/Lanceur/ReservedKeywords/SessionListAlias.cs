using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Splat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("sessions"), Description("list sessions")]
    public class SessionListAlias : SelfExecutableQueryResult
    {
        #region Fields

        private readonly IConversionService _converter;
        private readonly IDbRepository _service;

        #endregion Fields

        #region Constructors

        public SessionListAlias() : this(null)
        {
        }

        public SessionListAlias(IDbRepository service = null, IConversionService converter = null)
        {
            var l = Locator.Current;
            _service = service ?? l.GetService<IDbRepository>();
            _converter = converter ?? l.GetService<IConversionService>();
        }

        #endregion Constructors

        #region Properties

        public override string Icon => "AccountSwitch";

        #endregion Properties

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var sessions = _service.GetSessions();
            return Task.FromResult((IEnumerable<QueryResult>)_converter.ToSessionExecutableQueryResult(sessions));
        }

        #endregion Methods
    }
}