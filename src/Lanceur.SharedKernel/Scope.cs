namespace Lanceur.SharedKernel
{
    public class Scope<T> : IDisposable
    {
        #region Fields

        private readonly T _closeValue;
        private readonly T _openValue;

        #endregion Fields

        #region Constructors

        public Scope(Action<T> onStart, T openValue, T closeValue)
        {
            _openValue = openValue;
            _closeValue = closeValue;
            On = onStart;
        }

        #endregion Constructors

        #region Properties

        private Action<T> On { get; }

        #endregion Properties

        #region Methods

        public void Dispose() => On(_closeValue);

        public IDisposable Open()
        {
            On(_openValue);
            return this;
        }

        #endregion Methods
    }
}