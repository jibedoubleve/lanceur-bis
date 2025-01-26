using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Domain;

public class IniFileQuery
{
    #region Fields

    private readonly IEnumerable<IniNode> _nodes;
    private readonly ILogger<IniFileQuery> _logger;

    #endregion

    #region Constructors

    public IniFileQuery(IEnumerable<IniNode> nodes, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<IniFileQuery>();
        _nodes = nodes;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Attempts to locate the default profile path by searching through the INI files 
    /// that configure the profiles for Gecko-based browsers (e.g., Firefox).
    /// The method identifies the section related to installation and retrieves the default profile path specified in the configuration.
    /// </summary>
    /// <returns>
    /// A string representing the path to the default profile. If no default profile is found, an empty string is returned.
    /// </returns>
    public string GetDefaultProfile()
    {
        if (!_nodes.Any()) return string.Empty;
        
        var section = _nodes.Where(n => n.Section.StartsWith("Install"))
                            .Select(s => s.Section)
                            .FirstOrDefault();
        if (section == null) return string.Empty;

        var path = _nodes.Where(n => n.Section == section && n.Property == "Default")
                         .Select(n => n.Value.Replace("/", "\\"))
                         .First();
        _logger.LogTrace("Found default profile: {DefaultProfile}", path);
        return path;
    }

    #endregion
}