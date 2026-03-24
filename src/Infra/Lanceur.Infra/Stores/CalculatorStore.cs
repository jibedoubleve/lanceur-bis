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

    /// <inheritdoc cref="IStoreService.IsOverridable"/>
    public override bool IsOverridable => false;

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

    /// <inheritdoc cref="CanPruneResult" />
    public override bool CanPruneResult(Cmdline previous, Cmdline current) => false;

    /// <inheritdoc cref="CanPruneResult" />
    /// <remarks>
    ///     Always returns <c>0</c> — pruning is meaningless here because each expression evaluation
    ///     produces a single, self-contained result (the computed value or an error message) that is
    ///     unrelated to the previous result. <see cref="CanPruneResult" /> already prevents this
    ///     method from being called in normal operation.
    /// </remarks>
    public override int PruneResult(IList<QueryResult> destination, Cmdline? previous, Cmdline current) => 0;

    #endregion
}