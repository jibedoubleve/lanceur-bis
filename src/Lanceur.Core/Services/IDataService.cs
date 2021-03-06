using Lanceur.Core.Models;

namespace Lanceur.Core.Services
{
    public enum Per
    {
        Hour,
        Day,
        DayOfWeek,
        Month,
    }

    public interface IDataService
    {
        #region Methods

        /// <summary>
        /// Get all the aliases
        /// </summary>
        /// <param name="idSession">The session linked to the aliases. If null, it'll take the default session</param>
        /// <returns>All the aliases related to the specified session (or the default one if not specified).</returns>
        IEnumerable<AliasQueryResult> GetAll(long? idSession = null);

        Session GetDefaultSession();

        long GetDefaultSessionId();

        /// <summary>
        /// Get the list of all the doubloons in the database
        /// </summary>
        /// <returns>The list of doubloons</returns>
        IEnumerable<QueryResult> GetDoubloons();

        /// <summary>
        /// Get the alias with the exact name.
        /// </summary>
        /// <param name="name">The name of the alias to find</param>
        /// <returns>The alias to find or null if do not exist</returns>
        AliasQueryResult GetExact(string name, long? idSession = null);

        IEnumerable<SelectableAliasQueryResult> GetInvalidAliases();

        /// <summary>
        /// Get list of all the aliases with count greater than 0 and from the specified session
        /// If no session is specified, it'll take the default one
        /// </summary>
        /// <param name="idSession"></param>
        /// <returns>The list of aliases</returns>
        IEnumerable<QueryResult> GetMostUsedAliases(long? idSession = null);

        IEnumerable<Session> GetSessions();

        /// <summary>
        /// Returns usage trends. The result is meant to be dislayed as a chart
        /// </summary>
        /// <param name="per">The level of the trend. Can be hour, day, day of week or month</param>
        /// <param name="idSession">The session used to retrieve the usage</param>
        /// <returns>Points of the chart</returns>
        IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per, long? idSession = null);

        void Remove(AliasQueryResult alias);

        void Remove(Session session);

        void Remove(IEnumerable<SelectableAliasQueryResult> doubloons);

        /// <summary>
        /// Create a new alias if its id is '0' to the specified session (if not specified, to the default session)
        /// If the id exists, it'll update with the new information
        /// </summary>
        /// <param name="alias">The alias to create or update. In case of creation, if id will be set</param>
        /// <param name="idSession">
        /// If the alias has to be created, it'll be linked to this session. This argument is ignored if the alias
        /// needs to be updated. If not specified, the default session is selected.
        /// </param>
        void SaveOrUpdate(ref AliasQueryResult alias, long? idSession = null);

        /// <summary>
        /// Search all the alias that correspond to the criterion and that are linked to the specified session.
        /// If session is omitted, the default session is selected
        /// </summary>
        /// <param name="criteria">Criteria to find the aliases</param>
        /// <param name="idSession">ID of the session</param>
        /// <returns>Resulting aliases</returns>
        IEnumerable<AliasQueryResult> Search(string criteria, long? idSession = null);

        void SetDefaultSession(long idSession);

        void SetUsage(AliasQueryResult alias);

        void Update(ref Session session);

        #endregion Methods
    }
}