using System.Globalization;

namespace Lanceur.SharedKernel.Utils;

public static class ThreadExtension
{
    extension(Thread thread)
    {
        #region Methods

        private ThreadCultureMemento CaptureCulture()
            => new() { Culture = thread.CurrentCulture, UiCulture = thread.CurrentUICulture };

        private void RestoreCulture(ThreadCultureMemento memento)
        {
            thread.CurrentCulture = memento.Culture;
            thread.CurrentUICulture = memento.UiCulture;
        }

        private void UseInvariantCulture()
        {
            thread.CurrentCulture = CultureInfo.InvariantCulture;
            thread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        ///     Creates a scope that temporarily sets the thread's culture to <see cref="CultureInfo.InvariantCulture" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="IDisposable" /> that restores the original culture when disposed.
        /// </returns>
        /// <remarks>
        ///     The culture is restored automatically when the returned object is disposed, typically via a using statement.
        /// </remarks>
        public IDisposable UseInvariantCultureScope() => new ScopedCulture(thread);

        #endregion
    }

    private class ScopedCulture : IDisposable
    {
        #region Fields

        private readonly ThreadCultureMemento _memento;

        private readonly Thread _thread;

        #endregion

        #region Constructors

        public ScopedCulture(Thread thread)
        {
            _thread = thread;
            _memento = thread.CaptureCulture();
            _thread.UseInvariantCulture();
        }

        #endregion

        #region Methods

        public void Dispose() { _thread.RestoreCulture(_memento); }

        #endregion
    }
}