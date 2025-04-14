using System.Security.Cryptography;
using System.Text;

namespace Lanceur.Ui.Core.Utils;

public static class Enigma
{
    #region Methods

    public static string Decrypt(string encryptedText)
    {
        var encryptedBytesFromDb = Convert.FromBase64String(encryptedText);
        var decryptedBytes = ProtectedData.Unprotect(encryptedBytesFromDb, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string Encrypt(string plainText)
    {
        var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    #endregion
}