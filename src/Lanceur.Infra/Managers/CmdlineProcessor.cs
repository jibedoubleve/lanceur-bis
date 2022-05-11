using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Managers
{
    public class CmdlineProcessor : ICmdlineProcessor
    {
        #region Fields

        private static readonly char[] Specials = { 
            '$', '&', '|', '@', '#', '(', ')', '§', '!', '{', '}', '-', '_', '\\', '+', '-', '*', '/', '=', '<', '>', ',', ';', ':', '%', '?'
        };

        #endregion Fields

        #region Methods

        private static bool HasSpecialName(string cmdName)
        {
            if (cmdName?.Length > 0)
            {
                return Specials.Contains(cmdName[0]);
            }
            else { return false; }
        }

        public Cmdline Process(string cmdLine)
        {
            var name = string.Empty;
            var args = string.Empty;
            cmdLine ??= string.Empty;

            if (HasSpecialName(cmdLine))
            {
                name = cmdLine[0].ToString();
                args = cmdLine[1..];

            }
            else
            {

                var elements = cmdLine.Split(' ');
                if (elements.Length > 0)
                {
                    name = elements[0];
                    args = cmdLine[name.Length..];
                }

            }
            return new Cmdline(name, args.Trim());
        }

        #endregion Methods
    }
}