namespace Lanceur.Core.Models
{
    public abstract class ExecutableWithPrivilege : ExecutableQueryResult, IExecutableWithPrivilege
    {
        #region Properties

        public virtual bool IsPrivilegeOverriden { get; set; } = false;

        #endregion Properties
    }
}