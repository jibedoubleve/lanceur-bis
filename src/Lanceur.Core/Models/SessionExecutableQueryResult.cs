using Lanceur.Core.Services;

namespace Lanceur.Core.Models
{
    public class SessionExecutableQueryResult : ExecutableQueryResult
    {
        #region Fields

        private readonly ILogService _log;
        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public SessionExecutableQueryResult(
            string name,
            string description,
            ILogService log,
            IDataService service) : base(name, description)
        {
            _log = log;
            _service = service;
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(string parameters = null)
        {
            _log.Trace($"Change to session '{Name}' with id {Id}");
            _service.SetDefaultSession(Id);
            return NoResultAsync;
        }

        #endregion Methods
    }
}