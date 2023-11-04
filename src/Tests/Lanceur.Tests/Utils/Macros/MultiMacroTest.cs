﻿using Lanceur.Core;
using Lanceur.Core.Models;
using System.ComponentModel;

namespace Lanceur.Tests.Utils.Macros
{
    [Macro("multi"), Description("Allow to start multiple alias at once")]
    public class MultiMacroTest : SelfExecutableQueryResult
    {
        #region Constructors

        public MultiMacroTest() : this(null) { }
        
        public MultiMacroTest(string parameters = null)
        {
            Name = Guid.NewGuid().ToString();
            Parameters = parameters;
        }

        #endregion Constructors

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var list = new List<QueryResult>
            {
                new DisplayQueryResult(cmdline.Name, cmdline.Parameters)
            };
            return Task.FromResult<IEnumerable<QueryResult>>(list);
        }

        #endregion Methods
    }
}