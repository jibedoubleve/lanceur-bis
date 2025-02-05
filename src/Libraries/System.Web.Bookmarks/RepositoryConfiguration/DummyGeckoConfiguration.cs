namespace System.Web.Bookmarks.RepositoryConfiguration;

/// <inheritdoc />
internal class DummyGeckoConfiguration : IGeckoBrowserConfiguration
{
    #region Constructors

    public DummyGeckoConfiguration(string database, string iniFilename, string cacheKey)
    {
        Database = database;
        IniFilename = iniFilename;
        CacheKey = cacheKey;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public string CacheKey { get;  }

    /// <inheritdoc />
    public string Database { get;  }

    /// <inheritdoc />
    public string IniFilename { get;  }

    #endregion
}

internal class DummyChromiumConfiguration : IChromiumBrowserConfiguration
{
    public string CacheKey => Guid.NewGuid().ToString();
    public string Path => System.IO.Path.GetRandomFileName();
}