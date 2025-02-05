using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories;

public enum Per
{
    HourOfDay,
    Day,
    DayOfWeek,
    Month,
    Year
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
    ///     Retrieves an <see cref="AliasQueryResult" /> object based on its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="AliasQueryResult" />. It must be greater than zero.</param>
    /// <returns>
    ///     An <see cref="AliasQueryResult" /> object that matches the specified <paramref name="id" />
    ///     Returns null if no matching object is found.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when the <paramref name="id" /> is less than or equal to zero.
    /// </exception>
    AliasQueryResult GetById(long id);

    /// <summary>
    ///     Retrieves a collection of aliases that have been logically deleted.
    ///     Logical deletion means the aliases are marked as deleted in the system
    ///     but are not permanently removed from the database.
    /// </summary>
    /// <returns>
    ///     An <see cref="IEnumerable{SelectableAliasQueryResult}" /> representing the logically deleted aliases.
    /// </returns>
    IEnumerable<SelectableAliasQueryResult> GetDeletedAlias();

    /// <summary>
    ///     Get the list of all the doubloons in the database
    /// </summary>
    /// <returns>The list of doubloons</returns>
    IEnumerable<SelectableAliasQueryResult> GetDoubloons();

    /// <summary>
    ///     Checks which aliases from the provided list exist in the database.
    /// </summary>
    /// <param name="names">A list of aliases to check for existence in the database.</param>
    /// <param name="idAlias">Id of the alias to validate</param>
    /// <returns>An IEnumerable containing the aliases that exist in both the provided list and the database.</returns>
    public IEnumerable<string> GetExistingAliases(IEnumerable<string> names, long idAlias);

    /// <summary>
    ///     Retrieves aliases from the provided list that exist in the database and are marked as logically deleted.
    /// </summary>
    /// <param name="aliasesToCheck">A collection of alias names to verify against the database.</param>
    /// <param name="idAlias">The unique identifier of the alias being validated.</param>
    /// <returns>
    ///     An <see cref="IEnumerable{string}" /> containing aliases that are both present in the provided list
    ///     and marked as logically deleted in the database.
    /// </returns>
    IEnumerable<string> GetExistingDeletedAliases(IEnumerable<string> aliasesToCheck, long idAlias);

    /// <summary>
    ///     Retrieves counters for hidden but non-deleted aliases.
    /// </summary>
    /// <returns>
    ///     A dictionary where each key represents the file name of a hidden (but not deleted) alias,
    ///     and each value is a tuple containing the alias ID and its associated counter.
    /// </returns>
    Dictionary<string, (long Id, int Counter)> GetHiddenCounters();

    /// <summary>
    ///     Get list of all the aliases with count greater than 0
    /// </summary>
    /// <returns>The list of aliases</returns>
    IEnumerable<QueryResult> GetMostUsedAliases();

    /// <summary>
    ///     Retrieves a list of aliases with a count greater than 0 for the specified year.
    /// </summary>
    /// <param name="year">The year for which aliases are being retrieved.</param>
    /// <returns>
    ///     A collection of <see cref="QueryResult" /> objects, where each item contains the alias name and its associated
    ///     count.
    /// </returns>
    IEnumerable<QueryResult> GetMostUsedAliases(int year);

    /// <summary>
    ///     Returns usage trends. The result is meant to be dislayed as a chart
    /// </summary>
    /// <param name="per">The level of the trend. Can be hour, day, day of week or month</param>
    /// <returns>Points of the chart</returns>
    IEnumerable<DataPoint<DateTime, double>> GetUsage(Per per);

    /// <summary>
    ///     Retrieves all unique years where usage data is recorded.
    /// </summary>
    /// <returns>
    ///     An IEnumerable of integers representing the years with recorded usage.
    ///     The years are expected to be unique and may or may not be sorted, depending on the implementation.
    /// </returns>
    IEnumerable<int> GetYearsWithUsage();

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
    void Remove(IEnumerable<AliasQueryResult> aliases);

    /// <summary>
    ///     Restores the specified aliases by reversing their logical deletion status.
    ///     Aliases that are not marked as logically deleted will remain unchanged.
    /// </summary>
    /// <param name="aliases">
    ///     An array of <see cref="SelectableAliasQueryResult" /> objects representing the aliases to restore.
    ///     Only aliases marked as logically deleted will be affected.
    /// </param>
    void Restore(IEnumerable<SelectableAliasQueryResult> aliases);

    /// <summary>
    ///     Creates a new alias if the ID is '0'; otherwise, updates the existing alias.
    /// </summary>
    /// <param name="alias">The alias to create or update.</param>
    /// <returns>The ID of the created or updated alias.</returns>
    void SaveOrUpdate(ref AliasQueryResult alias);

    /// <summary>
    ///     Updates the database by applying the specified alias changes.
    ///     If an alias does not exist, it is created; otherwise, it is updated with the new values.
    /// </summary>
    /// <param name="aliases">
    ///     A collection of <see cref="AliasQueryResult" /> objects representing the aliases to be saved or updated.
    ///     Each object in the collection contains the details of a single alias, including its updated values.
    /// </param>
    void SaveOrUpdate(IEnumerable<AliasQueryResult> aliases);

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
    ///     Searches for a hidden alias with the specified name. and, if exists, set the usage into the specified QueryResult's
    ///     count property
    /// </summary>
    /// <param name="alias">The hidden alias to search for.</param>
    /// <returns>
    ///     The usage of the alias if found; otherwise, an empty <see cref="AliasUsage" /> object with its Id set to '0'.
    /// </returns>
    void SetHiddenAliasUsage(QueryResult alias);

    /// <summary>
    ///     Adds an entry to the usage table with the alias and the current date and time,
    ///     and updates the counter of the specified QueryResult.
    /// </summary>
    /// <remarks>
    ///     This method has a side effect: it modifies the counter of the provided alias.
    ///     If the counter is negative, no usage is recorded or saved in the history, and the counter remains hidden from the
    ///     user.
    /// </remarks>
    /// <param name="alias">The QueryResult object representing the alias to be updated. Must not be null.</param>
    void SetUsage(QueryResult alias);

    /// <summary>
    ///     Updates the thumbnail for the specified alias.
    /// </summary>
    /// <param name="alias">The alias to update.</param>
    /// <returns>The IDs of the updated aliases.</returns>
    /// <remarks>
    ///     This method does not create new aliases. If the ID of the alias is 0, 
    ///     no update will occur as the alias is considered non-existent. 
    ///     This method will only update the thumbnail; all other properties will be ignored.
    /// </remarks>
    void UpdateThumbnail(AliasQueryResult alias);


    #endregion
}