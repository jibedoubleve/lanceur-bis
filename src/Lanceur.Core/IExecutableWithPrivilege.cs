namespace Lanceur.Core
{
    public interface IExecutableWithPrivilege : IExecutable
    {
        #region Properties

        bool IsPrivileged { get; set; }

        #endregion Properties
    }
}