using System.Reflection;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;
using NCalc;

namespace Lanceur.Infra.Services;

/// <inheritdoc />
public class NCalcCalculatorService : ICalculatorService
{
    #region Fields

    private readonly ILogger<NCalcCalculatorService> _logger;

    private static readonly string[] Operators
        = typeof(Math).GetMethods(BindingFlags.Public | BindingFlags.Static)
                      .Select(m => m.Name)
                      .Distinct()
                      .ToArray();

    #endregion

    #region Constructors

    public NCalcCalculatorService(ILogger<NCalcCalculatorService> logger) => _logger = logger;

    #endregion

    #region Properties

    /// <inheritdoc />
    public string ValidationRegex => """
                                     (?i)^\s*\(?\s*(?:\d+|acos|acosh|asin|asinh|atan|atanh|atan2|cbrt|ceiling|cos|cosh|exp|floor|fusedmultiplyadd|log|log2|log10|pow|sin|sincos|sinh|sqrt|tan|tanh|abs|bigmul|bitdecrement|bitincrement|copysign|divrem|clamp|ieeeremainder|ilogb|max|maxmagnitude|min|minmagnitude|reciprocalestimate|reciprocalsqrtestimate|round|sign|truncate|scaleb)\b
                                     """;

    #endregion

    #region Methods

    private static string Normalize(string expression)
    {
        return Operators.Aggregate(
            expression,
            (current, op)
                => current.Replace(op.ToLower(), op)
        );
    }

    /// <inheritdoc />
    public (bool IsError, string Result) Evaluate(string expression)
    {
        try
        {
            expression = Normalize(expression);

            var exp = new Expression(expression);
            var result =  exp.Evaluate() ?? string.Empty;
            _logger.LogTrace("Calculation: {Expressuion} =>  {Result}", expression, result);
            return (false, $"{result}");
        }
        catch (Exception ex)
        {
            using  var _ = _logger.BeginSingleScope("Exception", ex);
            _logger.LogInformation("Error occured while handling expression '{Expression}", expression);
            return (true, ex.Message);
        }
    }

    #endregion
}