using System.Reflection;
using System.Windows.Input;

namespace Lanceur.Ui.Core.Utils;

public static class ViewModelExtensions
{
    #region Methods

    public static void NotifyCommandsUpdate(this object @this)
    {
        var type = @this.GetType();

        var properties = type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance
        );

        foreach (var property in properties)
            if (typeof(ICommand).IsAssignableFrom(property.PropertyType))
            {
                if (property.GetValue(@this) is not ICommand command) continue;

                var methodInfo = command.GetType().GetMethod("NotifyCanExecuteChanged");
                if (methodInfo is null)
                {
                    methodInfo = command.GetType().GetMethod("RaiseCanExecuteChanged");
                    if (methodInfo == null) continue;
                }

                methodInfo.Invoke(command, null);
            }
    }

    #endregion
}