namespace Lanceur.Infra.Stores.Everything;

public class EverythingQueryBuilder
{
    #region Fields

    private readonly HashSet<string> _prefixes = new();

    #endregion

    #region Methods

    public string BuildQuery() => string.Join(" ", _prefixes).Trim();

    public EverythingQueryBuilder ExcludeFilesInBin()
    {
        _prefixes.Add(EverythingModifiers.ExcludeFileInTrashBin);
        return this;
    }

    public EverythingQueryBuilder ExcludeHiddenFiles()
    {
        _prefixes.Add($"!{EverythingModifiers.IncludeHiddenFilesSwitch}");
        return this;
    }

    public EverythingQueryBuilder ExcludeSystemFiles()
    {
        _prefixes.Add($"!{EverythingModifiers.IncludeSystemFilesSwitch}");
        return this;
    }

    public EverythingQueryBuilder OnlyExecFiles()
    {
        _prefixes.Add(EverythingModifiers.OnlyExecFilesSwitch);
        return this;
    }

    #endregion
}