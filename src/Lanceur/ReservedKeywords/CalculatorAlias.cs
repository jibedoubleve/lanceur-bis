using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
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
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public CalculatorAlias(ICalculatorService calculator, IAppLoggerFactory log)
        {
            _calculator = calculator;
            _log = log.GetLogger<CalculatorAlias>();
        }

        public CalculatorAlias()
        {
            var l = Locator.Current;
            _calculator = l.GetService<ICalculatorService>();
            _log = l.GetLogger<CalculatorAlias>();
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            if (cmdline == null) { return Task.FromResult(DisplayQueryResult.SingleFromResult("Expression to evaluate is empty.")); }

            var parameters = cmdline.Parameters;
            _log.Trace($"Evaluating: {parameters}");
            return Task.FromResult(
                parameters.IsNullOrEmpty()
                    ? DisplayQueryResult.SingleFromResult("Not an expression to evaluate.")
                    : DisplayQueryResult.SingleFromResult(_calculator.Evaluate(parameters), parameters)
            );
        }

        #endregion Methods
    }
}