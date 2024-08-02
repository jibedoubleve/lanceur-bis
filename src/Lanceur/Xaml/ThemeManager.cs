using ControlzEx.Theming;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Splat;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Lanceur.Xaml;

internal class ThemeManager
{
    #region Fields

    private const string DarkTheme = "Dark.Accent1";
    private const string LightTheme = "Light.Accent1";
    private static ThemeManager _instance;
    private readonly ILogger<ThemeManager> _logger;
    private Application _app;

    #endregion Fields

    #region Constructors

    private ThemeManager(Application app)
    {
        _logger = Locator.Current.GetService<ILoggerFactory>().GetLogger<ThemeManager>();
        _app = app;

        var lightThemeUri = new Uri("pack://application:,,,/Xaml/Themes/LightTheme.xaml");
        var darkThemeUri = new Uri("pack://application:,,,/Xaml/Themes/DarkTheme.xaml");

        ControlzEx.Theming.ThemeManager.Current.AddLibraryTheme(
            new(lightThemeUri, CustomLibraryThemeProvider.DefaultInstance)
        );

        ControlzEx.Theming.ThemeManager.Current.AddLibraryTheme(
            new(darkThemeUri, CustomLibraryThemeProvider.DefaultInstance)
        );

        ResetTheme();
        ControlzEx.Theming.ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
        ControlzEx.Theming.ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
    }

    #endregion Constructors

    #region Enums

    public enum Themes { Light, Dark }

    #endregion Enums

    #region Properties

    public static ThemeManager Current
    {
        get
        {
            _instance ??= new(Application.Current);
            return _instance;
        }
    }

    public static bool IsLightTheme => GetTheme() == Themes.Light;

    #endregion Properties

    #region Methods

    private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
    {
        ControlzEx.Theming.ThemeManager.Current.ThemeChanged -= Current_ThemeChanged;
        try { ResetTheme(); }
        finally { ControlzEx.Theming.ThemeManager.Current.ThemeChanged += Current_ThemeChanged; }
    }

    private void ResetTheme() => SetTheme(GetTheme());

    public static Themes GetTheme()
    {
        var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\\Microsoft\Windows\\CurrentVersion\Themes\\Personalize", "AppsUseLightTheme", null);
        return value is null
            ? Themes.Light
            : Convert.ToBoolean(value)
                ? Themes.Light
                : Themes.Dark;
    }

    public void SetTheme(Themes? theme = null)
    {
        theme ??= GetTheme();
        var themeToApply = theme switch
        {
            Themes.Light => LightTheme,
            Themes.Dark  => DarkTheme,
            _            => throw new NotSupportedException($"The theme '{theme}' is not supported!")
        };

        _logger.LogTrace("Applying theme {ThemeToApply}. Requested theme is {Theme}", themeToApply, theme);
        ControlzEx.Theming.ThemeManager.Current.ChangeTheme(_app, themeToApply);
    }

    #endregion Methods
}

/// <summary>
/// Used to allow a default instantiable <see cref="LibraryThemeProvider"/>
/// </summary>
public class CustomLibraryThemeProvider : LibraryThemeProvider
{
    #region Fields

    public static readonly CustomLibraryThemeProvider DefaultInstance = new();

    #endregion Fields

    #region Constructors

    public CustomLibraryThemeProvider()
        : base(true) { }

    #endregion Constructors

    #region Methods

    /// <inheritdoc />
    public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues) { }

    #endregion Methods
}