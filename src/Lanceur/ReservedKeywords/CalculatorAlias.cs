using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("="), Description("Evaluate expressions such as quick calculations")]
    public class CalculatorAlias : ExecutableQueryResult
    {
        #region Fields

        private readonly ICalculatorService _calculator;
        private readonly ILogService _log;

        #endregion Fields

        #region Constructors

        public CalculatorAlias(ICalculatorService calculator, ILogService log)
        {
            _calculator = calculator;
            _log = log;
        }

        public CalculatorAlias()
        {
            var l = Locator.Current;
            _calculator = l.GetService<ICalculatorService>();
            _log = l.GetService<ILogService>();
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(string parameters = null)
        {
            _log.Trace($"Evaluating: {parameters}");
            var result = parameters.IsNullOrEmpty()
                ? DisplayQueryResult.SingleFromResult("Not an expression to evaluate.")
                : DisplayQueryResult.SingleFromResult(_calculator.Evaluate(parameters), parameters);
            return Task.FromResult(result);
        }

        #endregion Methods
    }
}