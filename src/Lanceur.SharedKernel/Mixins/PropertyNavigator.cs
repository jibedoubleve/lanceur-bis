namespace Lanceur.SharedKernel.Mixins;

/// <remarks>
/// https://stackoverflow.com/questions/1196991/get-property-value-from-string-using-reflection
/// </remarks>
public static class PropertyNavigator
{
    #region Methods

    public static object GetPropValue(this object obj, string name)
    {
        foreach (var part in name.Split('.'))
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var info = type.GetProperty(part);
            if (info == null) return null;

            obj = info.GetValue(obj, null);
        }

        return obj;
    }

    public static T GetPropValue<T>(this object obj, string name)
    {
        var retval = GetPropValue(obj, name);
        if (retval == null) return default;

        // throws InvalidCastException if types are incompatible
        return (T)retval;
    }

    #endregion Methods
}