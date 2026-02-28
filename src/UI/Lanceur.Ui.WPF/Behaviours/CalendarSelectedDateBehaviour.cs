using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Lanceur.Ui.WPF.Behaviours;

public class CalendarSelectedDateBehaviour : Behavior<Calendar>
{
    #region Fields

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(CalendarSelectedDateBehaviour),
        new(default(ICommand))
    );

    #endregion

    #region Properties

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    #endregion

    #region Methods

    private void OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        var date = e.AddedItems.Count > 0 ? e.AddedItems[0] as DateTime? : null;
        if (Command?.CanExecute(date) == true) { Command.Execute(date); }
    }

    protected override void OnAttached() { AssociatedObject.SelectedDatesChanged += OnSelectedDateChanged; }

    protected override void OnDetaching() { AssociatedObject.SelectedDatesChanged -= OnSelectedDateChanged; }

    #endregion
}