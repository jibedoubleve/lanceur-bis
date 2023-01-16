﻿using Lanceur.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace Lanceur.Xaml
{
    internal class ThemeHelper
    {
        #region Fields

        private static readonly Uri Dark = new("pack://application:,,,/Xaml/Themes/DarkTheme.xaml");
        private static readonly string DarkThemeTheme = "Dark.Accent1";
        private static readonly Uri Light = new("pack://application:,,,/Xaml/Themes/LightTheme.xaml");
        private static readonly string LightTheme = "Light.Accent1";

        #endregion Fields

        #region Methods

        public static bool IsLightTheme()
        {
            try
            {
                object value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", null);
                if (value == null)
                {
                    return true;
                }

                return Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                AppLogFactory.Get<ThemeHelper>().Error(ex.Message, ex);
            }

            return true;
        }

        public static void SetTheme()
        {
            var isLight = IsLightTheme();

            var theme = isLight ? Light : Dark;
            var themeName = isLight ? LightTheme : DarkThemeTheme;

            ControlzEx.Theming.ThemeManager.Current.AddLibraryTheme(new ControlzEx.Theming.LibraryTheme(theme, CustomLibraryThemeProvider.DefaultInstance));
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(App.Current, themeName);
        }

        #endregion Methods

        #region Classes

        private class CustomLibraryThemeProvider : ControlzEx.Theming.LibraryThemeProvider
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
            public override void FillColorSchemeValues(Dictionary<string, string> values, ControlzEx.Theming.RuntimeThemeColorValues colorValues)
            {
            }

            #endregion Methods
        }

        #endregion Classes
    }
}