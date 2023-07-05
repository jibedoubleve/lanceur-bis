using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class CalculatorStore : ISearchService
    {
        private static readonly ICalculatorService _calculator = new CodingSebCalculatorService();
        #region Methods

        public IEnumerable<QueryResult> GetAll()
        {
            return new List<QueryResult>();
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            var (isError, result) = _calculator.Evaluate(query.ToString());

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