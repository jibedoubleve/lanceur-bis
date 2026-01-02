using System.Text;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Scripting;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using NLua;

namespace Lanceur.Infra.Scripting;

public class LuaScriptEngine : IScriptEngine
{
    #region Fields

    private readonly IUserGlobalNotificationService _notificationService;

    #endregion

    #region Constructors

    public LuaScriptEngine(IUserGlobalNotificationService notificationService)
        => _notificationService = notificationService;

    #endregion

    #region Properties

    /// <inheritdoc/>
    public ScriptLanguage Language => ScriptLanguage.Lua;

    #endregion

    #region Methods

    /// <inheritdoc/>
    public Task<ScriptResult> ExecuteScriptAsync(Script script, bool isDebug = false)
    {
        if (script.Code.IsNullOrWhiteSpace())
        {
            return Task.FromResult(script.ToScriptResult());
        }

        using var lua = new Lua();
        try
        {
            var globals = new ScriptGlobals { Context = script.Context, Notification = new(_notificationService) };
            lua.State.Encoding = Encoding.UTF8;
            lua["context"] = globals.Context;
            lua["notification"] = globals.Notification;
            lua["logger"] = globals.Logger;
            var result = lua.DoString(script.Code);

            if (result.Length == 0) return Task.FromResult(script.ToScriptResult());

            var scriptResult = result[0] is not ScriptContext input
                ? script.ToScriptResult()
                : new()
                {
                    Code = script.Code,
                    Context = new() { FileName = input.FileName, Parameters = input.Parameters }
                };
            
            if(isDebug && !globals.Logger.IsEmpty) globals.Logger.Flush();
            
            return Task.FromResult(scriptResult);
        }
        catch (Exception ex)
        {
            var result = new ScriptResult { Code = script.Code, Exception = ex };
            return Task.FromResult(result);
        }
    }

    #endregion
}