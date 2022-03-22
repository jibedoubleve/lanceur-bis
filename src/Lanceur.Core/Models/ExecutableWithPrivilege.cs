namespace Lanceur.Core.Models
{
    public abstract class ExecutableWithPrivilege : ExecutableQueryResult, IExecutableWithPrivilege
    {
        #region Properties

        public virtual bool IsPrivileged { get; set; } = false;

        #endregion Properties
    }
}