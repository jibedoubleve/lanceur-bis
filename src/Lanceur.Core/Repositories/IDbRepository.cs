using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories;

public enum Per
{
    Hour,
    Day,
    DayOfWeek,
    Month
}

public interface IDbRepository
{
    #region Methods

    /// <summary>
    /// Get all the aliases
    /// </summary>
    /// <returns>All the aliases.</returns>
    IEnumerable<AliasQueryResult> GetAll();

    /// <summary>
    /// Get all the alias that has additional parameters.
    /// </summary>
    /// <returns>All the aliases.</returns>
    IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters();

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
    AliasQueryResult GetExact(string name);

    IEnumerable<SelectableAliasQueryResult> GetInvalidAliases();

    /// <summary>
    /// Search into the hidden aliases the one with the specified name and returns its ID
    /// </summary>
    /// <param name="name">The name of the hidden alias</param>
    /// <returns>The ID of this alias or '0' if not found</returns>
    KeywordUsage GetKeyword(string name);

    /// <summary>
    /// Get list of all the aliases with count greater than 0
    /// </summary>
    /// <returns>The list of aliases</returns>
    IEnumerable<QueryResult> GetMostUsedAliases();

    /// <summary>
    /// Returns usage trends. The result is meant to be dislayed as a chart
    /// </summary>
    /// <param name="per">The level of the trend. Can be hour, day, day of week or month</param>
    /// <returns>Points of the chart</returns>
    IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per);

    /// <summary>
    /// Update the id and the counter of <paramref name="queryResult"/>
    /// with the data of the database. It is used mainly to update
    /// plugins therfore it'll make an exact search on the name
    /// of the <see cref="QueryResult"/> and take the first item
    /// of the list (that should only have one item)
    /// </summary>
    /// <param name="queryResult">The query result to hydrate</param>
    void Hydrate(QueryResult queryResult);

    /// <summary>
    /// Hydrate the <see cref="AliasQueryResult"/> with the additional parameters.
    /// </summary>
    /// <param name="alias">The alias to hydrate</param>
    void HydrateAlias(AliasQueryResult alias);

    /// <summary>
    /// Hydrate the macro with its <c>id</c> and <c>count</c>. This method will try to find the
    /// macro by using its name  that should be something like '@it_s_name@'
    /// </summary>
    /// <param name="alias">Macro to hydrate</param>
    void HydrateMacro(QueryResult alias);

    /// <summary>
    /// Update the usage of the specified <see cref="QueryResult"/>
    /// </summary>
    /// <param name="result">The collection of <see cref="QueryResult"/>to refresh</param>
    /// <returns></returns>
    IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result);

    void Remove(AliasQueryResult alias);

    void Remove(IEnumerable<SelectableAliasQueryResult> doubloons);

    /// <summary>
    /// Creates a new alias if the ID is '0'; otherwise, updates the existing alias.
    /// </summary>
    /// <param name="alias">The alias to create or update.</param>
    /// <returns>The ID of the created or updated alias.</returns>
    void SaveOrUpdate(ref AliasQueryResult alias);

    /// <summary>
    /// Search all the alias that correspond to the criterion
    /// </summary>
    /// <param name="criteria">Criteria to find the aliases</param>
    /// <returns>Resulting aliases</returns>
    IEnumerable<AliasQueryResult> Search(string criteria);

    /// <summary>
    /// Search all the alias with additional parameters that correspond to the criterion 
    /// </summary>
    /// <param name="criteria">Criteria to find the aliases</param>
    /// <returns>Resulting aliases</returns>
    IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria);

    /// <summary>
    /// Returns all the names that exists in the database AND in the specified list of <see cref="names"/>
    /// </summary>
    /// <param name="names">The names to find in the database</param>
    /// <returns></returns>
    public ExistingNameResponse SelectNames(string[] names);

    /// <summary>
    /// Increment counter for execution of an alias. This is the recommended way to
    /// use counter for an executable alias
    /// </summary>
    /// <param name="alias"></param>
    void SetUsage(QueryResult alias);

    /// <summary>
    /// Updates thumbnail of many aliases at once.
    /// </summary>
    /// <param name="aliases">Aliases to update</param>
    /// <returns>The ids of the updated aliases</returns>
    /// <remarks>
    /// This method does not create new aliases. If the id of an alias is 0
    /// No update will occur as this alias is considered as non existing.
    /// 
    /// This methods will only update thumbnails, all other properties
    /// will be ignored.
    /// </remarks>
    void UpdateThumbnails(IEnumerable<AliasQueryResult> aliases);

    #endregion Methods
}