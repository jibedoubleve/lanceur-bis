using System.Collections;

namespace System.SQLite.Updater
{
    public class ScriptCollection : IEnumerable<string>
    {
        #region Fields

        private readonly IDictionary<Version, string> _resources;

        #endregion Fields

        #region Constructors

        public ScriptCollection(IDictionary<Version, string> resources)
        {
            if (resources is null) { throw new ArgumentNullException(nameof(resources)); }
            _resources = resources;
        }

        #endregion Constructors

        #region Indexers

        public string this[Version version]
        {
            get
            {
                return _resources.TryGetValue(version, out var value)
                    ? value
                    : string.Empty;
            }
        }

        #endregion Indexers

        #region Methods

        public IEnumerable<string> After(Version ver)
        {
            return from version in _resources.Keys
                   where version > ver
                   select _resources[version];
        }

        public IEnumerator<string> GetEnumerator() => _resources.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Version MaxVersion() => _resources?.Keys?.Max() ?? new Version();

        #endregion Methods
    }
}