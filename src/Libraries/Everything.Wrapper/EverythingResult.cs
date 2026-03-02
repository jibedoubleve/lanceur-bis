namespace Everything.Wrapper;

public record EverythingResult
{
    #region Properties

    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public ResultType ResultType { get; init; } = ResultType.File;

    #endregion
}