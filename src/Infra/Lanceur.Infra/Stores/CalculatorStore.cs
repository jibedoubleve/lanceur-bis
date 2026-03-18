using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public sealed class CalculatorStore : StoreBase, IStoreService
{
    #region Fields

    private readonly ICalculatorService _calculator;

    private readonly ILogger<CalculatorStore> _logger;

    #endregion

    #region Constructors

    public CalculatorStore(
        IStoreOrchestrationFactory orchestrationFactory,
        ILogger<CalculatorStore> logger,
        ICalculatorService calculator,
        ISection<StoreSection> storeSettings
    ) : base(orchestrationFactory, storeSettings)
    {
        ArgumentNullException.ThrowIfNull(calculator);

        _logger = logger;
        _calculator = calculator;
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

        var returnResult
            = new DisplayQueryResult(result, cmdline.ToString())
            {
                Icon = "Calculator24", 
                Count = -1
            };
        
        return isError
            ? QueryResult.NoResult
            : returnResult.ToEnumerable();
    }

    #endregion
}