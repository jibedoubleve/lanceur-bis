﻿using Lanceur.Core.Models;

namespace Lanceur.Core.LuaScripting
{
    public static class ScriptMixins
    {
        #region Methods

        public static Script Clone(this Script script, string scriptCode)
        {
            return new Script
            {
                Code = scriptCode,
                Context = script.Context,
            };
        }

        public static Script ToScript(this AliasQueryResult alias)
        {
            return new Script
            {
                Code = alias.LuaScript,
                Context = new ScriptContext
                {
                    FileName = alias.FileName,
                    Parameters = alias.Parameters
                }
            };
        }

        #endregion Methods
    }

    public class Script
    {
        #region Properties

        public string Code { get; init; }

        public ScriptContext Context { get; init; }

        public ScriptResult EmptyResult => new ScriptResult
        {
            Code = Code,
            Context = ScriptContext.Empty
        };

        #endregion Properties
    }
}