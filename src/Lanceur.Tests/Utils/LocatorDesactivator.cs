using NSubstitute;
using Splat;

namespace Lanceur.Tests.Utils
{
    internal class LocatorDesactivator : IDisposable
    {
        #region Fields

        private readonly IDependencyResolver _resolver;

        #endregion Fields

        #region Constructors

        public LocatorDesactivator()
        {
            _resolver = Locator.GetLocator();
            var emptyResolver = Substitute.For<IDependencyResolver>();
            Locator.SetLocator(emptyResolver);
        }

        #endregion Constructors

        #region Methods

        public void Dispose() => Locator.SetLocator(_resolver);

        #endregion Methods
    }
}