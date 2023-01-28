using ControlzEx.Theming;
using Lanceur.Core.Services;
using Lanceur.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Lanceur.Xaml
{
    internal class ThemeManager
    {
        #region Fields

        private const string DarkTheme = "Dark.Accent1";
        private const string LightTheme = "Light.Accent1";
        private static Application _app;
        private static readonly IAppLogger _log = AppLogFactory.Get<ThemeManager>();
        private static ThemeManager _instance;

        #endregion Fields

        #region Constructors

        private ThemeManager(Application app)
        {
            _app = app;

            var lightThemeUri = new Uri("pack://application:,,,/Xaml/Themes/LightTheme.xaml");
            var darkThemeUri = new Uri("pack://application:,,,/Xaml/Themes/DarkTheme.xaml");

            ControlzEx.Theming.ThemeManager.Current.AddLibraryTheme(
                new LibraryTheme(
                    lightThemeUri,
                    CustomLibraryThemeProvider.DefaultInstance)
            );

            ControlzEx.Theming.ThemeManager.Current.AddLibraryTheme(
                new LibraryTheme(
                    darkThemeUri,
                    CustomLibraryThemeProvider.DefaultInstance)
            );

            ResetTheme();
            ControlzEx.Theming.ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ControlzEx.Theming.ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
        }

        #endregion Constructors

        #region Enums

        public enum Themes
        {
            Light,
            Dark,
        }

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

        #endregion Properties

        #region Methods

        private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            ControlzEx.Theming.ThemeManager.Current.ThemeChanged -= Current_ThemeChanged;
            try
            {
                ResetTheme();
            }
            finally
            {
                ControlzEx.Theming.ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
            }
        }

        public static Themes GetTheme()
        {
            object value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\\Microsoft\Windows\\CurrentVersion\Themes\\Personalize", "AppsUseLightTheme", null);

            _log.Debug($"Actual theme is: '{(Convert.ToBoolean(value) ? "LIGHT" : "DARK")}'");

            return value is null
                ? Themes.Light
                : Convert.ToBoolean(value)
                    ? Themes.Light
                    : Themes.Dark;
        }

        private void ResetTheme() => SetTheme(GetTheme());

        public void SetTheme(Themes? theme = null)
        {
            theme ??= GetTheme();
            string themeToApply = theme switch
            {
                Themes.Light => LightTheme,
                Themes.Dark => DarkTheme,
                _ => throw new NotSupportedException($"The theme '{theme}' is not supported!")
            };

            _log.Debug($"Applying theme '{themeToApply}'. Asekd theme is '{theme}'");
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(_app, themeToApply);
        }

        #endregion Methods
    }

    /// <summary>
    /// Used to allow a default instanciable <see cref="LibraryThemeProvider"/>
    /// </summary>
    public class CustomLibraryThemeProvider : LibraryThemeProvider
    {
        #region Fields

        public static readonly CustomLibraryThemeProvider DefaultInstance = new();

        #endregion Fields

        #region Constructors

        public CustomLibraryThemeProvider()
                : base(true)
        {
        }

        #endregion Constructors

        #region Methods

        /// <inheritdoc />
        public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
        {
        }

        #endregion Methods
    }
}