using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Splat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{

    [ReservedAlias("sessions"), Description("list sessions")]
    public class SessionListAlias : ExecutableQueryResult
    {
        #region Fields

        private readonly IConvertionService _converter;
        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public SessionListAlias() : this(null, null)
        {
        }

        public SessionListAlias(IDataService service = null, IConvertionService converter = null)
        {
            var l = Locator.Current;
            _service = service ?? l.GetService<IDataService>();
            _converter = converter ?? l.GetService<IConvertionService>();
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var sessions = _service.GetSessions();
            return Task.FromResult((IEnumerable<QueryResult>)_converter.ToSessionExecutableQueryResult(sessions));
        }

        #endregion Methods
    }
}