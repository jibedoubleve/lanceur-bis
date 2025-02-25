using System.Drawing;
using System.IO;
using Lanceur.SharedKernel.Web;

namespace Lanceur.Infra.Win32.Services;

public class ImageValidationService : IImageValidationService

{
    #region Methods

    /// <inheritdoc />
    public bool IsValidImage(byte[] imageData)
    {
        try
        {
            using var ms = new MemoryStream(imageData);
            using var img = Image.FromStream(ms);
            return true;
        }
        catch { return false; }
    }

    #endregion
}