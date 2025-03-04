using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class CalculatorStore :Store, IStoreService
{
    #region Fields

    private readonly ILogger<CalculatorStore> _logger;

    private static readonly ICalculatorService Calculator = new CodingSebCalculatorService();

    #endregion

    #region Constructors

    public CalculatorStore(IServiceProvider serviceProvider) : base(serviceProvider) => _logger = serviceProvider.GetService<ILogger<CalculatorStore>>();

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Shared(@"^\s{0,}[0-9(]");

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        using var time = _logger.WarnIfSlow(this);
        var (isError, result) = Calculator.Evaluate(cmdline.ToString());

        /* Hack: if user search for 'gc' the result is
         *       "CodingSeb.ExpressionEvaluator.ClassOrEnumType"
         *       To fix this problem, I check whether the result
         *       is numeric and if it is not the case I consider
         *       that there's no result to display. */
        if (!float.TryParse(result, out _)) return QueryResult.NoResult;

        var returnResult = new DisplayQueryResult(result, cmdline.ToString()) { Icon = "calculator", Count = int.MaxValue };
        return isError
            ? QueryResult.NoResult
            : returnResult.ToEnumerable();
    }

    #endregion
}