namespace Lanceur.Core.Repositories;

/// <summary>
/// Provides tools for data correction and reconciliation.
/// </summary>
public interface IDataDoctorRepository
{
    #region Methods

    /// <summary>
    /// Sets the thumbnail to <c>null</c> for all aliases.
    /// </summary>
    void ClearThumbnails();

    #endregion Methods
}