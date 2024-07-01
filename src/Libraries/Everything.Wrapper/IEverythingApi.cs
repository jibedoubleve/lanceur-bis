namespace Everything.Wrapper;

public interface IEverythingApi
{
    /// <summary>
    /// Search files that matches the specified query
    /// </summary>
    /// <param name="query">Query for the search</param>
    /// <param name="isIconActivated">
    /// When <c>True</c>, the search will check the extension and try to infer the icon to apply;
    /// When <c>False</c>, the result will only return whether the result is a file or a directory
    /// </param>
    /// <returns>The result of the query</returns>
    /// <remarks>
    /// If you activate the icons, then the query is <c>much</c> slower.
    /// </remarks>
    ResultSet Search(string query, bool isIconActivated);
}