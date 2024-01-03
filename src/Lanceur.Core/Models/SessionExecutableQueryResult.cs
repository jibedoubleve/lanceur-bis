using Lanceur.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Lanceur.Core.Models
{
    public class SessionExecutableQueryResult : SelfExecutableQueryResult
    {
        #region Fields

        private readonly ILogger<SessionExecutableQueryResult> _logger;
        private readonly IDbRepository _service;

        #endregion Fields

        #region Constructors

        public SessionExecutableQueryResult(
            string name,
            string description,
            ILogger<SessionExecutableQueryResult> logger,
            IDbRepository service) : base(name, description)
        {
            _logger = logger;
            _service = service;
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            _logger.LogInformation("Change to session {Name} with id {Id}", Name, Id);
            _service.SetDefaultSession(Id);
            return NoResultAsync;
        }

        #endregion Methods
    }
}