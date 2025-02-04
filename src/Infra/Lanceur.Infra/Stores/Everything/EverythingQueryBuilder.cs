namespace Lanceur.Infra.Stores.Everything;

public class EverythingQueryBuilder
{
    #region Fields

    private readonly HashSet<string> _prefixes = new();

    #endregion

    #region Methods

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

    public override string ToString() => string.Join(" ", _prefixes).Trim();

    #endregion
}