using CodingSeb.ExpressionEvaluator;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services
{
    public class CodingSebCalculatorService : ICalculatorService
    {
        #region Methods

        public (bool IsError, string Result) Evaluate(string expression)
        {
            try
            {
                var evaluator = new ExpressionEvaluator
                {
                    OptionCaseSensitiveEvaluationActive = false,
                    OptionForceIntegerNumbersEvaluationsAsDoubleByDefault = true
                };

                var result = evaluator.Evaluate(expression);
                return (false, result.ToString());
            }
            catch (Exception ex) { return (true, ex.Message); }
        }

        #endregion Methods
    }
}