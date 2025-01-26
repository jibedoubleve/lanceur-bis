using System.Text.RegularExpressions;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Domain;

public class IniFileLoader
{
    #region Fields

    private readonly ILogger<IniFileLoader> _logger;

    private readonly ILoggerFactory _loggerFactory;

    private const string IsSectionPattern = @"^\[(.*)\]";

    #endregion

    #region Constructors

    public IniFileLoader(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<IniFileLoader>();
        _loggerFactory = loggerFactory;
    }

    #endregion

    #region Methods

    private IEnumerable<IniNode> Load(string filename)
    {
        if(!File.Exists(filename))return [];
        
        List<IniNode> nodes = new();
        var isSection = new Regex(IsSectionPattern);
        var currentSection = string.Empty;
        foreach (var line in File.ReadAllLines(filename))
        {
            if (line.IsNullOrWhiteSpace()) continue;

            if (isSection.IsMatch(line))
            {
                currentSection = isSection.Match(line).Groups[1].Value;
                continue;
            }

            var row = line.Split("=");
            nodes.Add(new(currentSection, row[0], row[1]));
        }
        
        return nodes;
    }

    public IniFileQuery LoadQuery(string filename) => new(Load(filename), _loggerFactory);

    #endregion
}