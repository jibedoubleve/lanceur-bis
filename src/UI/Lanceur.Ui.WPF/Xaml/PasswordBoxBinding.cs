using System.Windows;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Xaml;

public static class PasswordBoxBinding
{
    #region Fields

    public static readonly DependencyProperty BoundPassword =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxBinding),
            new(string.Empty, OnBoundPasswordChanged)
        );

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxBinding));

    #endregion

    #region Methods

    private static bool GetIsUpdating(DependencyObject obj) => (bool)obj.GetValue(IsUpdatingProperty);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox) return;

        passwordBox.PasswordChanged -= OnPasswordChanged;
        if (!GetIsUpdating(passwordBox)) passwordBox.Password = (string)e.NewValue;
        passwordBox.PasswordChanged += OnPasswordChanged;
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox) return;

        SetIsUpdating(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetIsUpdating(passwordBox, false);
    }

    private static void SetIsUpdating(DependencyObject obj, bool value) { obj.SetValue(IsUpdatingProperty, value); }

    public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPassword);

    public static void SetBoundPassword(DependencyObject obj, string value) { obj.SetValue(BoundPassword, value); }

    #endregion
}