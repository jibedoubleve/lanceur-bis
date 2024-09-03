namespace Lanceur.Core.Services;

/// <summary>
/// Provides a service for temporarily storing and retrieving text data in memory.
/// This service abstracts and replaces the standard Windows clipboard mechanism.
/// </summary>
public interface IMemoryStorageService
{
    /// <summary>
    /// Saves the specified text in memory for temporary storage.
    /// </summary>
    /// <param name="text">The text to be stored in memory.</param>
    public void SaveText(string text);

    /// <summary>
    /// Retrieves the previously stored text from memory.
    /// </summary>
    /// <returns>The text that was stored in memory.</returns>
    public string RetrieveText();
}
