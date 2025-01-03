using Lanceur.Core.Models;

namespace Lanceur.Core.BusinessLogic;

/// <summary>
///     This adapter transforms raw text into a collection of additional parameters for an alias.
///     Each line of the input text represents a parameter. The line format should be:
///     <c>name,content</c>
///     Where <c>name</c> is the parameter's name and <c>content</c> is its associated value.
///     If a line contains invalid data, such as multiple commas, it will be ignored.
/// </summary>
public class TextToParameterParser
{
    #region Methods

    private static bool TryParseLine(string line, out AdditionalParameter parameter)
    {
        var parts = line.Split(",");
        if (parts.Length > 2)
        {
            parameter = new();
            return false;
        }

        parameter =  new() { Name = parts[0].Trim(), Parameter = parts[1].Trim() };
        return true;
    }

   
    /// <summary>
    ///     Parses a block of text into a collection of <see cref="AdditionalParameter" /> objects.
    ///     Each line in the text is processed as an individual parameter.
    ///     If any line is invalid (such as containing more than one comma), an error result is returned.
    /// </summary>
    /// <param name="text">The block of text containing parameter lines.</param>
    /// <returns>A <see cref="ParseResult" /> indicating either success with the parsed parameters or an error.</returns>
    public ParseResult Parse(string text)
    {
        var parameters = new List<AdditionalParameter>();
        foreach (var line in text.Split(Environment.NewLine))
        {
            if (!TryParseLine(line, out var parameter)) return ParseResult.Failed();
            parameters.Add(parameter);
        }

        return ParseResult.Succeeded(parameters);
    }

    #endregion
}

public record ParseResult
{
    #region Constructors

    private ParseResult(IEnumerable<AdditionalParameter> parameters, bool success)
    {
        Parameters = parameters;
        Success = success;
    }

    #endregion

    #region Properties

    public IEnumerable<AdditionalParameter> Parameters { get; init; }
    public bool Success { get; init; }

    #endregion

    #region Methods

    internal static ParseResult Succeeded(IEnumerable<AdditionalParameter> parameters) => new(parameters, true);

    internal static ParseResult Failed() => new(null, false);

    #endregion
}