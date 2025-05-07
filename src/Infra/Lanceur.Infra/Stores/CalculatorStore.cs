using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class CalculatorStore : Store, IStoreService
{
    #region Fields

    private readonly ICalculatorService _calculator;

    private readonly ILogger<CalculatorStore> _logger;

    #endregion

    #region Constructors

    public CalculatorStore(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _logger = serviceProvider.GetService<ILogger<CalculatorStore>>();
        _calculator = serviceProvider.GetService<ICalculatorService>()
            ?? throw new NullReferenceException("ICalculatorService is null");
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Shared(_calculator.ValidationRegex);

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        using var time = _logger.WarnIfSlow(this);

        var (isError, result) = _calculator.Evaluate(cmdline.ToString());

        var returnResult = new DisplayQueryResult(result, cmdline.ToString()) { Icon = "calculator", Count = int.MaxValue };
        return isError
            ? QueryResult.NoResult
            : returnResult.ToEnumerable();

    }

    #endregion
}