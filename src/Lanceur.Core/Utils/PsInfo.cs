namespace Lanceur.Core.Utils;

/// <summary>
/// Represents a structure for storing information about a file and its related properties.
/// </summary>
/// <remarks>
/// This class is designed to encapsulate basic information about a file, such as its name, 
/// description, and an associated window handle, which can be useful in applications dealing 
/// with file management or UI components.
/// </remarks>
public struct PsInfo
{
    #region Properties

    /// <summary>
    /// Gets or sets the full path of the file.
    /// </summary>
    public string FileName { get; init; }
    
    /// <summary>
    /// Gets or sets the description of the file.
    /// This typically contains metadata about the file, such as its purpose or version information.
    /// </summary>
    public string FileDescription { get; init; }
    
    /// <summary>
    /// Gets or sets the handle of a window.
    /// This is often used to reference a specific UI element or window in the context of a graphical application.
    /// </summary>
    public string HWnd { get; init; }

    #endregion Properties
}