using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Infra.LuaScripting;
using Xunit;

namespace Lanceur.Tests.Functional;

public class LuaManagerShould
{
    #region Methods

    [Fact]
    public void NotCrashWhenScriptIsNull()
    {
        var result = LuaManager.ExecuteScript(new()
        {
            Code = null,
            Context = new()
            {
                FileName = null,
                Parameters = null
            }
        });
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.Context.FileName.Should().NotBeNull();
        result.Context.Parameters.Should().NotBeNull();
    }

    #endregion Methods
}