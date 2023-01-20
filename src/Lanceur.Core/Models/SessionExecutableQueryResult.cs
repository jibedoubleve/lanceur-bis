using Lanceur.Core.Services;

namespace Lanceur.Core.Models
{
    public class SessionExecutableQueryResult : ExecutableQueryResult
    {
        #region Fields

        private readonly IAppLogger _log;
        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public SessionExecutableQueryResult(
            string name,
            string description,
            IAppLoggerFactory logFactory,
            IDataService service) : base(name, description)
        {
            _log = logFactory.GetLogger<SessionExecutableQueryResult>();
            _service = service;
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            _log.Trace($"Change to session '{Name}' with id {Id}");
            _service.SetDefaultSession(Id);
            return NoResultAsync;
        }

        #endregion Methods
    }
}