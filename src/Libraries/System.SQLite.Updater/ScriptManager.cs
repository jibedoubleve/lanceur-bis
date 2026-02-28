using System.Reflection;
using System.Text.RegularExpressions;

namespace System.SQLite.Updater;

public partial class ScriptManager
{
    #region Fields

    private readonly Assembly _asm;
    private readonly Regex _regex;

    #endregion

    #region Constructors

    public ScriptManager(Assembly asm, string pattern)
    {
        ArgumentNullException.ThrowIfNull(asm);
        ArgumentNullException.ThrowIfNull(pattern);

        _regex = new(pattern);
        _asm = asm;
    }

    #endregion

    #region Methods

    [GeneratedRegex(@"^.*?(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*")]
    private static partial Regex RegexSelectVersion();

    public string GetResource(string resourceName)
    {
        using var stream = _asm.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new ArgumentNullException(
                nameof(resourceName),
                $"'{nameof(resourceName)}' is null. Are you sure the resource '{resourceName}' exists in the assembly '{_asm.FullName}'"
            );
        }

        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public IDictionary<string, string> GetResources() => ListResources().ToDictionary(item => item, GetResource)!;

    public ScriptCollection GetScripts()
    {
        var resources = GetResources();
        var src = new SortedDictionary<Version, string>();

        foreach (var item in resources)
        {
            var match = RegexSelectVersion().Matches(item.Key);
            if (match.Count <= 0 || match[0].Groups.Count < 1) { continue; }

            var ver = match[0].Groups[1].Value.Trim('.');
            var version = new Version(ver);
            src.Add(version, item.Value);
        }

        var ordered = src.OrderBy(x => x.Key);
        return new(new Dictionary<Version, string>(ordered));
    }

    public IEnumerable<string> ListResources()
        => _asm.GetManifestResourceNames()
               .Where(s => _regex.IsMatch(s));

    #endregion
}