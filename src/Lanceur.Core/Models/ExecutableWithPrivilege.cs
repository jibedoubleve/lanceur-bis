namespace Lanceur.Core.Models
{
    public abstract class ExecutableWithPrivilege : SelfExecutableQueryResult, IElevated
    {
        #region Properties

        public virtual bool IsElevated { get; set; } = false;

        #endregion Properties
    }
}