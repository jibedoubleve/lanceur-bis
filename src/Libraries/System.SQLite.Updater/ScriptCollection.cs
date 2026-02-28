using System.Collections;

namespace System.SQLite.Updater;

public class ScriptCollection : IEnumerable<string>
{
    #region Fields

    private readonly IDictionary<Version, string> _resources;

    #endregion

    #region Constructors

    public ScriptCollection(IDictionary<Version, string> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);

        _resources = resources;
    }

    #endregion

    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<string> After(Version version)
        => _resources.Where(item => item.Key > version)
                     .Select(item => item.Value);

    public IEnumerator<string> GetEnumerator() => _resources.Values.GetEnumerator();

    public Version MaxVersion() => _resources.Keys.Max() ?? new Version();

    #endregion

    #region Indexers

    public string this[Version version] => _resources.TryGetValue(version, out var value)
        ? value
        : string.Empty;

    #endregion Indexers
}