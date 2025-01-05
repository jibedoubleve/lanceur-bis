using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories;

public enum Per
{
    HourOfDay,
    Day,
    DayOfWeek,
    Month
}

public interface IAliasRepository
{
    #region Methods

    /// <summary>
    ///     Retrieves additional parameters for the specified alias IDs.
    /// </summary>
    /// <param name="ids">A collection of alias IDs to retrieve additional parameters for.</param>
    /// <returns>A collection of QueryResultAdditionalParameters containing the additional parameters for each specified ID.</returns>
    IEnumerable<AdditionalParameter> GetAdditionalParameter(IEnumerable<long> ids);

    /// <summary>
    ///     Retrieves all aliases in the system that don't have associated notes or comments.
    /// </summary>
    /// <returns>
    ///     A collection of selectable aliases without notes, where each item contains
    ///     the alias details and selection state.
    /// </returns>
    /// <remarks>
    ///     This method filters out any aliases that have notes attached to them,
    ///     making it useful for identifying aliases that may need additional documentation.
    /// </remarks>
    IEnumerable<SelectableAliasQueryResult> GetAliasesWithoutNotes();

    /// <summary>
    ///     Get all the aliases
    /// </summary>
    /// <returns>All the aliases.</returns>
    IEnumerable<AliasQueryResult> GetAll();

    /// <summary>
    ///     Get all the alias that has additional parameters.
    /// </summary>
    /// <returns>All the aliases.</returns>
    IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters();

    /// <summary>
    ///     Retrieves an <see cref="AliasQueryResult" /> object based on its unique identifier.
    ///     The <paramref name="name" /> is used to filter the results, ensuring that only one result is returned for the given
    ///     <paramref name="id" />.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="AliasQueryResult" />. It must be greater than zero.</param>
    /// <param name="name">The name used to filter the results corresponding to the specified <paramref name="id" />.</param>
    /// <returns>
    ///     An <see cref="AliasQueryResult" /> object that matches the specified <paramref name="id" /> and
    ///     <paramref name="name" />.
    ///     Returns null if no matching object is found.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when the <paramref name="id" /> is less than or equal to zero.
    /// </exception>
    AliasQueryResult GetByIdAndName(long id, string name);

    /// <summary>
    ///     Get the list of all the doubloons in the database
    /// </summary>
    /// <returns>The list of doubloons</returns>
    IEnumerable<SelectableAliasQueryResult> GetDoubloons();

    /// <summary>
    ///     Get the alias with the exact name.
    /// </summary>
    /// <param name="name">The name of the alias to find</param>
    /// <returns>The alias to find or null if do not exist</returns>
    AliasQueryResult GetExact(string name);

    /// <summary>
    ///     Checks which aliases from the provided list exist in the database.
    /// </summary>
    /// <param name="names">A list of aliases to check for existence in the database.</param>
    /// <param name="idAlias">Id of the alias to validate</param>
    /// <returns>An IEnumerable containing the aliases that exist in both the provided list and the database.</returns>
    public IEnumerable<string> GetExistingAliases(IEnumerable<string> names, long idAlias);
    
    /// <summary>
    ///     Retrieves a collection of aliases that are incorrectly configured,
    ///     indicating they are invalid. A broken alias is an alias that has a filename
    ///     that leads to a non-existing file.
    /// </summary>
    /// <returns>
    ///     An enumerable collection of <see cref="SelectableAliasQueryResult" />
    ///     representing the aliases that have been poorly configured and are therefore invalid.
    ///     These invalid aliases are considered "broken" because their associated filenames
    ///     point to non-existing files.
    /// </returns>
    IEnumerable<SelectableAliasQueryResult> GetBrokenAliases();


    /// <summary>
    ///     Search into the hidden aliases the one with the specified name and returns its ID
    /// </summary>
    /// <param name="name">The name of the hidden alias</param>
    /// <returns>The ID of this alias or '0' if not found</returns>
    KeywordUsage GetKeyword(string name);

    /// <summary>
    ///     Get list of all the aliases with count greater than 0
    /// </summary>
    /// <returns>The list of aliases</returns>
    IEnumerable<QueryResult> GetMostUsedAliases();

    /// <summary>
    ///     Returns usage trends. The result is meant to be dislayed as a chart
    /// </summary>
    /// <param name="per">The level of the trend. Can be hour, day, day of week or month</param>
    /// <returns>Points of the chart</returns>
    IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per);

    /// <summary>
    ///     Update the id and the counter of <paramref name="queryResult" />
    ///     with the data of the database. It is used mainly to update
    ///     plugins therfore it'll make an exact search on the name
    ///     of the <see cref="QueryResult" /> and take the first item
    ///     of the list (that should only have one item)
    /// </summary>
    /// <param name="queryResult">The query result to hydrate</param>
    void Hydrate(QueryResult queryResult);

    /// <summary>
    ///     Hydrate the <see cref="AliasQueryResult" /> with the additional parameters.
    /// </summary>
    /// <param name="alias">The alias to hydrate</param>
    void HydrateAlias(AliasQueryResult alias);

    /// <summary>
    ///     Hydrate the macro with its <c>id</c> and <c>count</c>. This method will try to find the
    ///     macro by using its name  that should be something like '@it_s_name@'
    /// </summary>
    /// <param name="alias">Macro to hydrate</param>
    void HydrateMacro(QueryResult alias);

    /// <summary>
    ///     Update the usage of the specified <see cref="QueryResult" />
    /// </summary>
    /// <param name="result">The collection of <see cref="QueryResult" />to refresh</param>
    /// <returns></returns>
    IEnumerable<QueryResult> RefreshUsage(IEnumerable<QueryResult> result);

    /// <summary>
    ///     Marks the specified alias as removed from the repository. 
    ///     This is a logical removal; the alias remains in the database 
    ///     but is flagged as deleted and excluded from subsequent queries.
    /// </summary>
    /// <param name="alias">The alias to mark as removed from the repository.</param>
    void Remove(AliasQueryResult alias);

    /// <summary>
    ///     Marks the specified list of aliases as removed from the repository. 
    ///     This is a logical removal; the aliases remain in the database 
    ///     but are flagged as deleted and excluded from subsequent queries.
    /// </summary>
    /// <param name="aliases">The list of aliases to mark as removed from the repository.</param>
    void RemoveMany(IEnumerable<AliasQueryResult> aliases);


    /// <summary>
    ///     Creates a new alias if the ID is '0'; otherwise, updates the existing alias.
    /// </summary>
    /// <param name="alias">The alias to create or update.</param>
    /// <returns>The ID of the created or updated alias.</returns>
    void SaveOrUpdate(ref AliasQueryResult alias);


    /// <summary>
    ///     Searches for all aliases that match the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria used to search for aliases.</param>
    /// <param name="isReturnAllIfEmpty">
    ///     Specifies the behavior when <paramref name="criteria" /> is null or empty:
    ///     if <c>true</c>, all aliases are returned; if <c>false</c>, no results are returned.
    /// </param>
    /// <returns>A collection of aliases that match the search criteria.</returns>
    IEnumerable<AliasQueryResult> Search(string criteria, bool isReturnAllIfEmpty = false);

    /// <summary>
    ///     Search all the alias with additional parameters that correspond to the criterion
    /// </summary>
    /// <param name="criteria">Criteria to find the aliases</param>
    /// <returns>Resulting aliases</returns>
    IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string criteria);

    /// <summary>
    ///     Returns all the names that exists in the database AND in the specified list of <see cref="names" />
    /// </summary>
    /// <param name="names">The names to find in the database</param>
    /// <returns></returns>
    public ExistingNameResponse SelectNames(string[] names);

    /// <summary>
    ///     Increments the execution counter for a given alias.
    ///     This method is the recommended way to track usage of an executable alias.
    /// </summary>
    /// <param name="alias">The alias whose execution is being tracked.</param>
    void SetUsage(QueryResult alias);

    /// <summary>
    ///     Updates the database with the changes specified in the provided alias records.
    /// </summary>
    /// <param name="aliases">
    ///     A collection of AliasQueryResult objects containing the alias changes to apply.
    ///     Each item in the collection represents a single alias and its updated values.
    /// </param>
    void Update(IEnumerable<AliasQueryResult> aliases);

    /// <summary>
    ///     Updates thumbnail of many aliases at once.
    /// </summary>
    /// <param name="aliases">Aliases to update</param>
    /// <returns>The ids of the updated aliases</returns>
    /// <remarks>
    ///     This method does not create new aliases. If the id of an alias is 0
    ///     No update will occur as this alias is considered as non existing.
    ///     This methods will only update thumbnails, all other properties
    ///     will be ignored.
    /// </remarks>
    void UpdateThumbnails(IEnumerable<AliasQueryResult> aliases);

    #endregion
}