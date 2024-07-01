namespace Everything.Wrapper;

public record Result
{
    #region Properties

    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public ResultType ResultType { get; init; } = ResultType.File;

    #endregion Properties
}