using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
/// Implementation of the IAliasService interface for managing aliases.
/// Stores aliases in a Dictionary with their corresponding targets.
/// </summary>
public interface IAliasManagementService
{
    /// <summary>
    /// Retrieves all aliases
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="AliasQueryResult"/> objects, 
    /// representing the aliases and any associated information.
    /// </returns>
    IEnumerable<AliasQueryResult> GetAll();

    /// <summary>
    /// Populates or "hydrates" the provided <see cref="AliasQueryResult"/> instance 
    /// with additional parameters that may not be initially included.
    /// </summary>
    /// <param name="queryResult">
    /// The <see cref="AliasQueryResult"/> object to be filled with any additional parameters.
    /// </param>
    /// <returns>
    /// A fully populated <see cref="AliasQueryResult"/> object, containing
    /// any extra parameters that were added during the hydration process.
    /// </returns>
    AliasQueryResult Hydrate(AliasQueryResult queryResult);


    /// <summary>
    /// Deletes the specified query result from the database.
    /// </summary>
    /// <param name="alias">The query result object to be deleted.</param>
    void Delete(AliasQueryResult alias);

    /// <summary>
    /// Saves or updates the specified alias query result to the database. If the entity is newly created, 
    /// its Id is updated after saving.
    /// </summary>
    /// <param name="alias">The alias query result object to be saved or updated. The Id will be updated if 
    /// the entity is newly created in the database.</param>
    void SaveOrUpdate(ref AliasQueryResult alias);

}