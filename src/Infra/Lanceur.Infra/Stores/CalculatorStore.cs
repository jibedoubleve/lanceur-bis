using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class CalculatorStore : IStoreService
{
    #region Fields

    private readonly ILogger<CalculatorStore> _logger;

    private static readonly ICalculatorService Calculator = new CodingSebCalculatorService();

    #endregion

    #region Constructors

    public CalculatorStore(IServiceProvider serviceProvider) => _logger = serviceProvider.GetService<ILogger<CalculatorStore>>();

    #endregion

    #region Properties

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestration.Shared(@"^\s{0,}[0-9(]");

    #endregion

    #region Methods

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

    #endregion
}