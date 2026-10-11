using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EzItam.Web.Services;

public static class EnumDisplay
{
    public static string Name(Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var attr = member?.GetCustomAttribute<DisplayAttribute>();
        return attr?.GetName() ?? value.ToString();
    }

    public static IEnumerable<(TEnum Value, string Name)> All<TEnum>() where TEnum : struct, Enum
        => Enum.GetValues<TEnum>().Select(v => (v, Name(v)));
}
