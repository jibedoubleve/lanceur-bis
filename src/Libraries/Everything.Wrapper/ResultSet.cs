using System.Collections;

namespace Everything.Wrapper;

public class ResultSet : IEnumerable<Result>
{
    #region Fields

    private readonly List<Result> _results = new();

    #endregion Fields

    #region Constructors

    public ResultSet(string query) => Query = query;

    #endregion Constructors

    #region Properties

    public int Count => _results.Count;
    public string Query { get; }

    #endregion Properties

    #region Methods

    public void Add(Result result) => _results.Add(result);

    public IEnumerator<Result> GetEnumerator() => _results.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Methods
}