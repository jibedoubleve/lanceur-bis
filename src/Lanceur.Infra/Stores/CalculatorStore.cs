using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class CalculatorStore : ISearchService
    {
        #region Fields

        private static readonly ICalculatorService Calculator = new CodingSebCalculatorService();
        private readonly ILogger<CalculatorStore> _logger;

        #endregion Fields

        #region Constructors

        public CalculatorStore()
        {
            _logger = Locator.GetLocator()
                             .GetService<ILoggerFactory>()
                             .CreateLogger<CalculatorStore>();
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<QueryResult> GetAll()
        {
            return new List<QueryResult>();
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            using var time = _logger.MeasureExecutionTime(this);
            var (isError, result) = Calculator.Evaluate(query.ToString());

            /* Hack: if user search for 'gc' the result is
             *       "CodingSeb.ExpressionEvaluator.ClassOrEnumType"
             *       To fix this problem, I check whether the result
             *       is numeric and if it is not the case I consider
             *       that there's no result to display. */
            if (!float.TryParse(result, out _)) { return QueryResult.NoResult; }
            if (isError) { return QueryResult.NoResult; }

            return new DisplayQueryResult(result, query.ToString())
            {
                Count = int.MaxValue,
                Icon = "calculator"
            }.ToEnumerable();
        }

        #endregion Methods
    }
}