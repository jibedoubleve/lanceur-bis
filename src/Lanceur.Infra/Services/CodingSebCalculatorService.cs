using CodingSeb.ExpressionEvaluator;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services
{
    public class CodingSebCalculatorService : ICalculatorService
    {
        #region Methods

        public string Evaluate(string expression)
        {
            try
            {
                var evaluator = new ExpressionEvaluator();
                var result = evaluator.Evaluate(expression);
                return result.ToString();
            }
            catch (Exception ex) { return ex.Message; }
        }

        #endregion Methods
    }
}