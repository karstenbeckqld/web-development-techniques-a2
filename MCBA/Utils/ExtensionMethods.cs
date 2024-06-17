using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MCBA.Utils;

// The following extension methods have been kindly provided by Dr Matthew Bolger during the lectures and tutorials.
public static class ExtensionMethods
{
    public static bool HasMoreThanNDecimalPlaces(this decimal value, int n) => decimal.Round(value, n) != value;
    public static bool HasMoreThanTwoDecimalPlaces(this decimal value) => value.HasMoreThanNDecimalPlaces(2);

    public static List<string> getStateEnumList() => ((Enums.StateEnum[])Enums.StateEnum.GetValues(typeof(Enums.StateEnum))).Select(x => x.ToString()).ToList();

    public static List<string> getPayPeriodEnum() => ((Enums.billPayEnum[])Enums.billPayEnum.GetValues(typeof(Enums.billPayEnum))).Select(x => x.ToString()).ToList();

    public static string GetDisplayName(this Enum value)
    {
        return value.GetType()
          .GetMember(value.ToString())
          .First()
          .GetCustomAttribute<DisplayAttribute>()
          ?.GetName();
    }

}