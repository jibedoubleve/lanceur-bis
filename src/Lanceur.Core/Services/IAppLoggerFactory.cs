namespace Lanceur.Core.Services
{
    public interface IAppLoggerFactory
    {
        #region Methods

        IAppLogger GetLogger<TSource>();

        IAppLogger GetLogger(Type sourceType);

        #endregion Methods
    }
}