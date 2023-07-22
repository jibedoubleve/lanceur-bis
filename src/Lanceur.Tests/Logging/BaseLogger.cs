using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public class BaseLogger
    {
        #region Fields

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        public BaseLogger(ITestOutputHelper output)
        {
            ArgumentNullException.ThrowIfNull(nameof(output));
            _output = output;
        }

        #endregion Constructors

        #region Methods

        protected void Write(string message, [CallerMemberName] string method = null) => _output.WriteLine($"[{method,-6}] {message}");

        protected void Write(string message, Exception ex, [CallerMemberName] string method = null) => _output.WriteLine($"[{method,-6}] {message} - {ex}");

        protected void Write(Exception ex, [CallerMemberName] string method = null) => _output.WriteLine($"[{method,-6}] {ex}");

        #endregion Methods
    }
}