namespace Lanceur.SharedKernel;

public class Constants
{
    #region Enums

    /// <summary>
    /// Granted right for the application to launch
    /// </summary>
    /// <remarks>
    /// The order of the items in this <see cref="Enum"/> is very important
    /// as it is mapped from the SQLite database:
    ///  - 0 is for Admin
    ///  - 1 is for CurrentUser
    /// </remarks>
    public enum RunAs { Admin, CurrentUser }

    /// <summary>
    /// Indicates how the application should be started
    /// </summary>
    /// <remarks>
    /// The order of the items in this <see cref="Enum"/> is very important
    /// as it is mapped from the SQLite database:
    ///  - 0 is for Default
    ///  - 1 is for Maximized
    ///  - 2 is for Minimized
    /// </remarks>
    public enum StartMode { Default, Maximised, Minimised }

    #endregion Enums
}