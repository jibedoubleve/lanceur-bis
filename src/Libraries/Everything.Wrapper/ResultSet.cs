using System.Collections;

namespace Everything.Wrapper;

public class ResultSet : IEnumerable<EverythingResult>
{
    #region Fields

    private readonly List<EverythingResult> _results = new();

    #endregion

    #region Constructors

    public ResultSet() : this(false, string.Empty) { }

    private ResultSet(bool hasError, string errorMessage)
    {
        HasError = hasError;
        ErrorMessage = errorMessage;
    }

    #endregion

    #region Properties

    public int Count => _results.Count;

    public string ErrorMessage { get; }

    public bool HasError { get; }

    #endregion

    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(EverythingResult everythingResult) => _results.Add(everythingResult);

    public static ResultSet Failure(string message) => new(true, message);

    public IEnumerator<EverythingResult> GetEnumerator() => _results.GetEnumerator();

    #endregion
}