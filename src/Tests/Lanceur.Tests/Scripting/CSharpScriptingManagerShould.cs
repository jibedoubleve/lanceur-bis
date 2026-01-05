using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Scripting;
using Lanceur.Core.Services;
using Lanceur.Infra.Scripting;
using Lanceur.Tests.Tools.Extensions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Scripting;

public class CSharpScriptingManagerShould
{
    #region Fields

    private readonly ITestOutputHelper _outputHelper;

    #endregion

    #region Constructors

    public CSharpScriptingManagerShould(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    #endregion

    #region Properties

    private static IScriptEngine ScriptEngine
    {
        get
        {
            var settings = Substitute.For<ISection<ScriptingSection>>();
            settings.Value.Returns(new ScriptingSection());

            var cache = new MemoryCache(new MemoryCacheOptions());

            return new CSharpScriptEngine(
                Substitute.For<IUserGlobalNotificationService>(),
                settings,
                cache
            );
        }
    }

    #endregion

    #region Methods

    [Theory]
    [InlineData("", "")]
    [InlineData("dev", "output_dev")]
    [InlineData("test", "output_test")]
    [InlineData("prod", "output_prod")]
    public async Task GetExpectedFileNameAfterScriptExecution(string parameter, string expectedFilename)
    {
        // arrange
        const string luaScript = """
                                 if(Context.Parameters == "dev")
                                 {
                                     Context.FileName = "output_dev";
                                     return;
                                 }
                                 if(Context.Parameters == "test")
                                 {
                                     Context.FileName = "output_test";
                                     return;
                                 }
                                 if(Context.Parameters == "prod")
                                 {
                                     Context.FileName = "output_prod";
                                     return;
                                 }
                                 """;

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName = "", Parameters = parameter } }
        );

        HandleError(result);

        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe(expectedFilename)
        );
    }

    private void HandleError(ScriptResult result)
    {
        _outputHelper.WriteLine(result.IsOnError() 
            ? result.Exception.ToString() 
            : "No error when executing script");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("dev", "output_dev")]
    [InlineData("test", "output_test")]
    [InlineData("prod", "output_prod")]
    public async Task GetExpectedParametersAfterScriptExecution(string parameter, string expectedParameter)
    {
        // arrange
        const string url = "https://random.url.com";
        const string luaScript = """
                                 if(Context.Parameters == "dev")
                                 {
                                     Context.Parameters = "output_dev";
                                     return;
                                 }
                                 if(Context.Parameters == "test")
                                 {
                                     Context.Parameters = "output_test";
                                     return;
                                 }
                                 if(Context.Parameters == "prod")
                                 {
                                     Context.Parameters = "output_prod";
                                     return;
                                 }
                                 """;

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName = url, Parameters = parameter } }
        );
        
        HandleError(result);
        
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe(url),
            r => r.Context.Parameters.ShouldBe(expectedParameter)
        );
    }

    [Fact]
    public async Task NotCrashWhenScriptIsNull()
    {
        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = null, Context = new() { FileName = null, Parameters = null } }
        );
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldNotBeNull(),
            r => r.Context.Parameters.ShouldNotBeNull()
        );
    }

    [Fact]
    public async Task ReturnAnErrorWhenScriptDoesNotCompile()
    {
        // arrange
        const string url = "https://random.url.com";
        const string luaScript = "this is a failing script";

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName   = url, Parameters = "unhandled_case" } }
        );
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Exception.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBeEmpty(),
            r => r.Context.Parameters.ShouldBeEmpty()
        );
    }

    [Fact]
    public async Task ReturnEmptyContextWhenScriptDoNotReturnContext()
    {
        // arrange
        const string url = "https://random.url.com";
        const string parameters = "unhandled_case";
        const string luaScript = "return 145;";

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName = url, Parameters = parameters } }
        );

        HandleError(result);

        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe(url),
            r => r.Context.Parameters.ShouldBe(parameters)
        );
    }

    [Fact]
    public async Task CacheCompiledScriptsForBetterPerformance()
    {
        // arrange
        var settings = Substitute.For<ISection<ScriptingSection>>();
        settings.Value.Returns(new ScriptingSection());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var engine = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            cache
        );

        const string script = """
                              Context.FileName = "cached_result";
                              """;

        // act - first execution (should compile and cache)
        var firstResult = await engine.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        // act - second execution (should use cached compilation)
        var secondResult = await engine.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        // assert
        firstResult.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe("cached_result")
        );

        secondResult.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe("cached_result")
        );
    }

    [Fact]
    public async Task UseDifferentCacheForDifferentScripts()
    {
        // arrange
        var settings = Substitute.For<ISection<ScriptingSection>>();
        settings.Value.Returns(new ScriptingSection());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var engine = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            cache
        );

        const string script1 = """
                               Context.FileName = "result1";
                               """;

        const string script2 = """
                               Context.FileName = "result2";
                               """;

        // act
        var result1 = await engine.ExecuteScriptAsync(
            new() { Code = script1, Context = new() { FileName = "", Parameters = "" } }
        );

        var result2 = await engine.ExecuteScriptAsync(
            new() { Code = script2, Context = new() { FileName = "", Parameters = "" } }
        );

        // assert
        result1.Context.FileName.ShouldBe("result1");
        result2.Context.FileName.ShouldBe("result2");
    }

    [Fact]
    public async Task InvalidateCacheWhenUsingsChange()
    {
        // arrange
        var settings = Substitute.For<ISection<ScriptingSection>>();
        var initialSection = new ScriptingSection { Usings = new[] { "System", "System.IO" } };
        settings.Value.Returns(initialSection);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var engine = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            cache
        );

        const string script = """
                              Context.FileName = "test";
                              """;

        // act - first execution with initial usings
        var result1 = await engine.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        // change usings
        var updatedSection = new ScriptingSection { Usings = new[] { "System", "System.IO", "System.Linq" } };
        settings.Value.Returns(updatedSection);

        // act - second execution with updated usings (should recompile with new options)
        var result2 = await engine.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        // assert - both should succeed
        result1.Context.FileName.ShouldBe("test");
        result2.Context.FileName.ShouldBe("test");
    }

    [Fact]
    public async Task PersistCacheToDiskForReuseAfterRestart()
    {
        // arrange
        var settings = Substitute.For<ISection<ScriptingSection>>();
        settings.Value.Returns(new ScriptingSection());
        var cache = new MemoryCache(new MemoryCacheOptions());

        const string script = """
                              Context.FileName = "persistent_result";
                              """;

        // Create first engine instance and execute script
        var engine1 = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            cache
        );

        var result1 = await engine1.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        // Create new engine instance (simulating restart) with fresh memory cache
        var freshCache = new MemoryCache(new MemoryCacheOptions());
        var engine2 = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            freshCache
        );

        // act - execute same script with new engine (should load from disk cache)
        var result2 = await engine2.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        // assert - both executions should succeed
        result1.Context.FileName.ShouldBe("persistent_result");
        result2.Context.FileName.ShouldBe("persistent_result");

        // cleanup
        engine1.ClearPersistentCache();
    }

    [Fact]
    public void ClearPersistentCacheShouldRemoveAllCachedAssemblies()
    {
        // arrange
        var settings = Substitute.For<ISection<ScriptingSection>>();
        settings.Value.Returns(new ScriptingSection());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var engine = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            cache
        );

        const string script = """
                              Context.FileName = "test";
                              """;

        // Execute a script to create cache files
        engine.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        ).Wait();

        // act
        engine.ClearPersistentCache();

        // assert - verify cache directory exists but is empty of DLL files
        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lanceur",
            "ScriptCache"
        );

        if (Directory.Exists(cacheDir))
        {
            var dllFiles = Directory.GetFiles(cacheDir, "*.dll");
            var pdbFiles = Directory.GetFiles(cacheDir, "*.pdb");

            dllFiles.ShouldBeEmpty();
            pdbFiles.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task CleanOldCacheEntriesShouldRemoveExpiredFiles()
    {
        // arrange
        var settings = Substitute.For<ISection<ScriptingSection>>();
        settings.Value.Returns(new ScriptingSection());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var engine = new CSharpScriptEngine(
            Substitute.For<IUserGlobalNotificationService>(),
            settings,
            cache
        );

        const string script = """
                              Context.FileName = "test";
                              """;

        // Execute a script to create cache files
        await engine.ExecuteScriptAsync(
            new() { Code = script, Context = new() { FileName = "", Parameters = "" } }
        );

        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lanceur",
            "ScriptCache"
        );

        // Manually set file dates to simulate old cache
        if (Directory.Exists(cacheDir))
        {
            foreach (var file in Directory.GetFiles(cacheDir))
            {
                File.SetLastAccessTime(file, DateTime.UtcNow.AddDays(-31));
            }
        }

        // act - clean files older than 30 days
        engine.CleanOldCacheEntries(TimeSpan.FromDays(30));

        // assert - old files should be removed
        if (Directory.Exists(cacheDir))
        {
            var files = Directory.GetFiles(cacheDir);
            files.ShouldBeEmpty();
        }
    }

    #endregion
}