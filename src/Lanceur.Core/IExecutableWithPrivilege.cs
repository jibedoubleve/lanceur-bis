namespace Lanceur.Core
{
    public interface IExecutableWithPrivilege : IExecutable
    {
        #region Properties

        bool IsPrivilegeOverriden { get; set; }

        #endregion Properties
    }
}