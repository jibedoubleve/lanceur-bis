using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Splat;

namespace Lanceur.Infra.Stores;

[Store]
public class CalculatorStore : IStorehService
{
    #region Fields

    private static readonly ICalculatorService Calculator = new CodingSebCalculatorService();
    private readonly ILogger<CalculatorStore> _logger;

    #endregion Fields

    #region Constructors


    public CalculatorStore(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILogger<CalculatorStore>>();
    }

    [Obsolete("Use ctor with service provider instead")]
    public CalculatorStore(ILoggerFactory loggerFactory = null)
    {
        loggerFactory ??= Locator.GetLocator().GetService<ILoggerFactory>();
        _logger = loggerFactory?.CreateLogger<CalculatorStore>() ?? new NullLogger<CalculatorStore>();
    }

    #endregion Constructors

    #region Methods

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.Shared(@"^\s{0,}[0-9(]");

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var time = _logger.MeasureExecutionTime(this);
        var (isError, result) = Calculator.Evaluate(query.ToString());

        /* Hack: if user search for 'gc' the result is
         *       "CodingSeb.ExpressionEvaluator.ClassOrEnumType"
         *       To fix this problem, I check whether the result
         *       is numeric and if it is not the case I consider
         *       that there's no result to display. */
        if (!float.TryParse(result, out _)) return QueryResult.NoResult;

        if (isError) return QueryResult.NoResult;

        return new DisplayQueryResult(result, query.ToString()) { Count = int.MaxValue, Icon = "calculator" }.ToEnumerable();
    }

    #endregion Methods
}