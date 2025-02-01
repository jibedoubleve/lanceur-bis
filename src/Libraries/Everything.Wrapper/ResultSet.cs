using System.Collections;

namespace Everything.Wrapper;

public class ResultSet : IEnumerable<EverythingResult>
{
    #region Fields

    private readonly List<EverythingResult> _results = new();

    #endregion Fields

    #region Constructors

    public ResultSet(string query) => Query = query;

    #endregion Constructors

    #region Properties

    public int Count => _results.Count;
    public string Query { get; }

    #endregion Properties

    #region Methods

    public void Add(EverythingResult everythingResult) => _results.Add(everythingResult);

    public IEnumerator<EverythingResult> GetEnumerator() => _results.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion Methods
}