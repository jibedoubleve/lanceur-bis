using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Scripting;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Script = Lanceur.Core.Scripting.Script;

namespace Lanceur.Infra.Scripting;

public class CSharpScriptEngine : IScriptEngine
{
    #region Fields

    private readonly IUserGlobalNotificationService _notificationService;
    private readonly ISection<ScriptingSection> _settings;

    #endregion

    #region Constructors

    public CSharpScriptEngine(
        IUserGlobalNotificationService notificationService,
        ISection<ScriptingSection> settings)
    {
        _notificationService = notificationService;
        _settings = settings;
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
            var options = ScriptOptions.Default
                                       .AddReferences(
                                           typeof(object).Assembly,    // mscorlib/System.Private.CoreLib
                                           typeof(Enumerable).Assembly // System.Linq
                                       )
                                       .AddImports(_settings.Value.Usings);
            using var _ = Thread.CurrentThread.UseInvariantCultureScope();
            await CSharpScript.EvaluateAsync(
                script.Code,
                options,
                globals,
                typeof(ScriptGlobals)
            );
            
            if(isDebug && !globals.Logger.IsEmpty) globals.Logger.Flush();
            
            return script.ToScriptResult(globals.Context);
        }
        catch (Exception ex) { return script.ToScriptError(ex); }
    }

    #endregion
}