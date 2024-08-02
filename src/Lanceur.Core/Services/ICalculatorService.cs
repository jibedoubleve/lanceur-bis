namespace Lanceur.Core.Services;

public interface ICalculatorService
{
    #region Methods

    (bool IsError, string Result) Evaluate(string expression);

    #endregion Methods
}