using Lanceur.SharedKernel.Extensions;
using Xunit;

namespace Lanceur.Tests.Tooling.Logging;

public class BaseTestOutputHelperDecorator
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    protected BaseTestOutputHelperDecorator(ITestOutputHelper output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    #endregion

    #region Methods

    protected void Write(Exception? ex, string message, object[] propertyValues)
    {
        var msg = $"{(propertyValues.Length == 0 ? message : message.Format(propertyValues))}";
        if (ex is not null) msg += $" - {ex}";
        _output.WriteLine(msg);
    }

    protected void Write(string message) => Write(null, message, Array.Empty<object>());

    #endregion
}