using System.Reflection;
using System.Text.RegularExpressions;

namespace Lanceur.Core.BusinessLogic;

/// <summary>
/// Provides functionality for validating macros and related operations.
/// </summary>
public class MacroValidator

{
    #region Fields

    private readonly IEnumerable<string> _macroNames;
    private static readonly Regex IsMacroRegex = new("^@.*@$");

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroValidator"/> class.
    /// </summary>
    /// <param name="asm">The <see cref="Assembly"/> from which to identify 
    /// types containing the <see cref="MacroAttribute"/>. It is expected that 
    /// this assembly contains types that have the <see cref="MacroAttribute"/> 
    /// applied.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided <paramref name="asm"/> parameter is null.
    /// </exception>
    public MacroValidator(Assembly asm)
    {
        ArgumentNullException.ThrowIfNull(asm);
        _macroNames = asm.GetTypes()
                         .Where(e => e.GetCustomAttributes<MacroAttribute>().Any())
                         .Select(
                             e => e.GetCustomAttribute<MacroAttribute>()!
                                   .Name
                                   .ToUpper()
                         );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether the specified file name appears to be a macro.
    /// This method checks if the file name is enclosed with '@' characters,
    /// but does not verify if the file name is present in a list of supported macros.
    /// </summary>
    /// <param name="fileName">The name of the file to check.</param>
    /// <returns>
    /// Returns true if the file name is formatted as a macro (i.e., it is surrounded by '@'); 
    /// otherwise, returns false.
    /// </returns>
    public bool IsMacroFormat(string fileName) => IsMacroRegex.IsMatch(fileName);

    /// <summary>
    /// Determines whether the specified fileName is a valid macro.
    /// A valid macro is surrounded by '@' symbols and exists in the list of predefined macros.
    /// </summary>
    /// <param name="fileName">The file name to validate as a macro, including the '@' symbols.</param>
    /// <returns>True if the fileName is a valid macro; otherwise, false.</returns>
    public bool IsValid(string fileName)
    {
        if (!IsMacroRegex.IsMatch(fileName)) return false;

        fileName = fileName.ToUpper();

        return _macroNames.All(e => e != fileName);
    }

    #endregion
}