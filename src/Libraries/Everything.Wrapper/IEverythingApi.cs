namespace Everything.Wrapper;

public interface IEverythingApi
{
    /// <summary>
    /// Search files that matches the specified query
    /// </summary>
    /// <param name="query">Query for the search</param>
    /// <returns>The result of the query</returns>
    /// <remarks>
    /// If you activate the icons, then the query is <c>much</c> slower.
    /// </remarks>
    ResultSet Search(string query);
}