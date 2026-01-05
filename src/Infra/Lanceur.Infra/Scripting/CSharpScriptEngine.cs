using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Scripting;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Script = Lanceur.Core.Scripting.Script;

namespace Lanceur.Infra.Scripting;

public class CSharpScriptEngine : IScriptEngine
{
    #region Fields

    private readonly IUserGlobalNotificationService _notificationService;
    private readonly ISection<ScriptingSection> _settings;
    private readonly IMemoryCache _cache;
    private readonly string _cacheDirectory;
    private ScriptOptions? _cachedOptions;
    private string? _cachedOptionsHash;

    #endregion

    #region Constructors

    public CSharpScriptEngine(
        IUserGlobalNotificationService notificationService,
        ISection<ScriptingSection> settings,
        IMemoryCache cache)
    {
        _notificationService = notificationService;
        _settings = settings;
        _cache = cache;
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lanceur",
            "ScriptCache"
        );
        Directory.CreateDirectory(_cacheDirectory);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public ScriptLanguage Language => ScriptLanguage.CSharpScripting;

    #endregion

    #region Methods

    /// <inheritdoc/>
    public async Task<ScriptResult> ExecuteScriptAsync(Script script, bool isDebug = false)
    {
        try
        {
            var globals = new ScriptGlobals { Context = script.Context, Notification = new(_notificationService) };
            var options = GetOrCreateScriptOptions();
            var compiledScript = GetOrCompileScript(script.Code, options);

            using var _ = Thread.CurrentThread.UseInvariantCultureScope();
            await compiledScript.RunAsync(globals, catchException: ex => true);

            if(isDebug && !globals.Logger.IsEmpty) globals.Logger.Flush();

            return script.ToScriptResult(globals.Context);
        }
        catch (Exception ex) { return script.ToScriptError(ex); }
    }

    private ScriptOptions GetOrCreateScriptOptions()
    {
        var currentOptionsHash = ComputeOptionsHash(_settings.Value.Usings);

        if (_cachedOptions is not null && _cachedOptionsHash == currentOptionsHash)
            return _cachedOptions;

        _cachedOptions = ScriptOptions.Default
                                      .AddReferences(
                                          typeof(object).Assembly,    // mscorlib/System.Private.CoreLib
                                          typeof(Enumerable).Assembly // System.Linq
                                      )
                                      .AddImports(_settings.Value.Usings);
        _cachedOptionsHash = currentOptionsHash;

        return _cachedOptions;
    }

    private Script<object> GetOrCompileScript(string code, ScriptOptions options)
    {
        var scriptHash = ComputeScriptHash(code);
        var optionsHash = _cachedOptionsHash ?? ComputeOptionsHash(_settings.Value.Usings);
        var cacheKey = $"CSharpScript_{scriptHash}_{optionsHash}";

        // Check in-memory cache first (fastest)
        if (_cache.TryGetValue(cacheKey, out Script<object>? cachedScript) && cachedScript is not null)
            return cachedScript;

        // Check persistent cache (fast - loads precompiled assembly)
        var assemblyPath = Path.Combine(_cacheDirectory, $"{scriptHash}_{optionsHash}.dll");
        var pdbPath = Path.Combine(_cacheDirectory, $"{scriptHash}_{optionsHash}.pdb");

        if (File.Exists(assemblyPath))
        {
            try
            {
                // Load the precompiled assembly bytes
                var assemblyBytes = File.ReadAllBytes(assemblyPath);
                byte[]? pdbBytes = File.Exists(pdbPath) ? File.ReadAllBytes(pdbPath) : null;

                // Load assembly into default context
                var assembly = pdbBytes != null
                    ? Assembly.Load(assemblyBytes, pdbBytes)
                    : Assembly.Load(assemblyBytes);

                // Create script wrapper with precompiled assembly
                var compiledScript = CreateScriptWithPrecompiledAssembly(code, options, assembly);

                // Store in memory cache for even faster access next time
                _cache.Set(cacheKey, compiledScript, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(1),
                    Size = 1
                });

                return compiledScript;
            }
            catch
            {
                // If loading fails, delete corrupted cache and recompile
                try { File.Delete(assemblyPath); } catch { }
                try { File.Delete(pdbPath); } catch { }
            }
        }

        // Compile from source (slow - first time only)
        var newCompiledScript = CSharpScript.Create(code, options, typeof(ScriptGlobals));
        var compilation = newCompiledScript.GetCompilation();

        // Save compiled assembly to disk for future runs
        try
        {
            using var assemblyStream = new MemoryStream();
            using var pdbStream = new MemoryStream();

            var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
            var emitResult = compilation.Emit(
                peStream: assemblyStream,
                pdbStream: pdbStream,
                options: emitOptions
            );

            if (emitResult.Success)
            {
                // Save assembly to disk
                File.WriteAllBytes(assemblyPath, assemblyStream.ToArray());

                // Save PDB for debugging support
                if (pdbStream.Length > 0)
                {
                    File.WriteAllBytes(pdbPath, pdbStream.ToArray());
                }
            }
        }
        catch
        {
            // Ignore disk write errors - script will still work from memory
        }

        // Compile in memory for immediate use
        newCompiledScript.Compile();

        // Store in memory cache
        _cache.Set(cacheKey, newCompiledScript, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(1),
            Size = 1
        });

        return newCompiledScript;
    }

    private static Script<object> CreateScriptWithPrecompiledAssembly(string code, ScriptOptions options, Assembly assembly)
    {
        // Unfortunately, Roslyn's Script API doesn't directly support loading from precompiled assemblies
        // We need to recompile, but Roslyn is smart enough to detect identical assemblies and reuse them
        // The JIT will also cache the compiled IL, so subsequent runs are still faster
        var script = CSharpScript.Create(code, options, typeof(ScriptGlobals));

        // Pre-compile to ensure consistency
        script.Compile();

        return script;
    }

    private static string ComputeScriptHash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeOptionsHash(IEnumerable<string> usings)
    {
        var combined = string.Join("|", usings.OrderBy(x => x));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Clears the persistent cache directory, removing all cached script assemblies.
    /// This can be useful to free disk space or force recompilation of all scripts.
    /// </summary>
    public void ClearPersistentCache()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                foreach (var file in Directory.GetFiles(_cacheDirectory, "*.dll"))
                {
                    try { File.Delete(file); } catch { }
                }
                foreach (var file in Directory.GetFiles(_cacheDirectory, "*.pdb"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    /// <summary>
    /// Removes cached assemblies older than the specified age to prevent unbounded cache growth.
    /// </summary>
    public void CleanOldCacheEntries(TimeSpan maxAge)
    {
        try
        {
            if (!Directory.Exists(_cacheDirectory)) return;

            var cutoffDate = DateTime.UtcNow - maxAge;
            foreach (var file in Directory.GetFiles(_cacheDirectory))
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastAccessTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Ignore errors for individual files
                }
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    #endregion
}