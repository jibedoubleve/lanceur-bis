namespace Lanceur.Infra.Stores.Everything;

public class EverythingQueryAdapter
{
    #region Fields

    private readonly string _query;

    #endregion

    #region Constructors

    public EverythingQueryAdapter(string query) => _query = query.ToLower();

    #endregion

    #region Properties

    public bool IsFilesInTrashBinExcluded => Contains(EverythingModifiers.ExcludeFileInTrashBin);

    public bool IsHiddenFilesExcluded => Contains($"!{EverythingModifiers.IncludeHiddenFilesSwitch}");
    public bool IsSystemFilesExcluded => Contains($"!{EverythingModifiers.IncludeSystemFilesSwitch}");
    public bool SelectOnlyExecutable => Contains(EverythingModifiers.OnlyExecFilesSwitch);

    #endregion

    #region Methods

    private bool Contains(string value) => _query.Contains(value);

    #endregion
}