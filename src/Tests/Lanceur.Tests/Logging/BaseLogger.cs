using System.Runtime.CompilerServices;
using Lanceur.SharedKernel.Mixins;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public class BaseLogger
    {
        #region Fields

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        protected BaseLogger(ITestOutputHelper output)
        {
            ArgumentNullException.ThrowIfNull(nameof(output));
            _output = output;
        }

        #endregion Constructors

        #region Methods

        protected void Write(Exception ex, string message,  object[] propertyValues, [CallerMemberName] string method = null)
        {
            var msg = $"[{method,-6}] {message.Format(propertyValues)}";
            if (ex is not null)
            {
                msg += $" - {ex}";
            }
            _output.WriteLine(msg);
        }

        #endregion Methods
    }
}