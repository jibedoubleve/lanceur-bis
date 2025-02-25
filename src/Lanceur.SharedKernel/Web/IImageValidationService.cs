namespace Lanceur.SharedKernel.Web;

/// <summary>
///     Provides functionality to validate image data.
/// </summary>
public interface IImageValidationService
{
    #region Methods

    /// <summary>
    ///     Determines whether the given byte array represents a valid image format.
    /// </summary>
    /// <param name="imageData">A byte array containing the image data.</param>
    /// <returns><c>true</c> if the byte array contains a valid image; otherwise, <c>false</c>.</returns>
    bool IsValidImage(byte[] imageData);

    #endregion
}