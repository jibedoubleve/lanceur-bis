namespace Lanceur.Core.Services;

/// <summary>
/// Provides methods for encrypting and decrypting text.
/// </summary>
public interface IEnigma
{
    #region Methods

    /// <summary>
    ///     Decrypts the specified encrypted text.
    /// </summary>
    /// <param name="encryptedText">The text to be decrypted.</param>
    /// <returns>The decrypted plain text.</returns>
    string Decrypt(string encryptedText);

    /// <summary>
    ///     Encrypts the specified plain text.
    /// </summary>
    /// <param name="plainText">The text to be encrypted.</param>
    /// <returns>The encrypted result as a string.</returns>
    string Encrypt(string plainText);

    #endregion
}