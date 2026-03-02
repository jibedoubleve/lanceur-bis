namespace Lanceur.Core.Services;

/// <summary>
///     Provides methods for evaluating mathematical expressions and validating their correctness.
/// </summary>
public interface ICalculatorService
{
    #region Properties

    /// <summary>
    ///     Gets a regular expression pattern used to validate mathematical expressions
    ///     before evaluation. This pattern defines the allowed syntax and format
    ///     for expressions that can be processed by the calculator.
    /// </summary>
    /// <remarks>
    ///     The regex helps ensure that input expressions contain only supported
    ///     mathematical operators, numbers, and valid syntax, reducing the risk of
    ///     evaluation errors.
    /// </remarks>
    /// <returns>
    ///     A string representing the regular expression pattern for expression validation.
    /// </returns>
    string ValidationRegex { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Evaluates a mathematical expression and returns the computed result.
    /// </summary>
    /// <param name="expression">
    ///     A string representing the mathematical expression to be evaluated.
    /// </param>
    /// <returns>
    ///     A tuple containing:
    ///     - <c>IsError</c>: A boolean indicating whether an error occurred during evaluation.
    ///     - <c>Result</c>: A string representing the computed value if successful, or an error message if unsuccessful.
    /// </returns>
    (bool IsError, string Result) Evaluate(string expression);

    #endregion
}