namespace System.Web.Bookmarks.Configuration;

/// <inheritdoc />
public class DummyGeckoConfiguration : IGeckoBrowserConfiguration
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