namespace Lanceur.Core.Services
{
    public interface IAppLogger
    {
        #region Methods

        void Debug(string message, params object[] propertyValues);

        void Debug(Exception ex);

        void Error(Exception ex, string message, params object[] propertyValues);

        void Fatal(Exception ex, string message, params object[] propertyValues);

        void Info(string message, params object[] propertyValues);

        void Trace(string message, params object[] propertyValues);

        void Warning(string message, params object[] propertyValues);

        void Warning(Exception ex, string message, params object[] propertyValues);

        void Warning(Exception ex);

        #endregion Methods
    }
}