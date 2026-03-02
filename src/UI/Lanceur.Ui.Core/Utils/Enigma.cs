using System.Security.Cryptography;
using System.Text;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.Utils;

public class Enigma : IEnigma
{
    #region Methods

    /// <inheritdoc />
    public  string Decrypt(string encryptedText)
    {
        var encryptedBytesFromDb = Convert.FromBase64String(encryptedText);
        var decryptedBytes = ProtectedData.Unprotect(encryptedBytesFromDb, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <inheritdoc />
    public  string Encrypt(string plainText)
    {
        var encryptedBytes = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(plainText),
            null,
            DataProtectionScope.CurrentUser
        );
        return Convert.ToBase64String(encryptedBytes);
    }

    #endregion
}