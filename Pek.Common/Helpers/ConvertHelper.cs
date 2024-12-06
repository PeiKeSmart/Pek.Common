using System.Globalization;

using Pek.Timing;

namespace Pek.Helpers;

public class ConvertHelper
{
    public static T? ToType<T>(String value)
    {
        Object? obj = default(T);
        T? result;
        if (String.IsNullOrEmpty(value))
        {
            result = (T?)obj;
        }
        else
        {
            obj = ToType(value, typeof(T));
            result = (T?)obj;
        }
        return result;
    }

    public static T? ToType<T>(String value, T defaultValue)
    {
        T? result;
        if (String.IsNullOrEmpty(value))
        {
            result = defaultValue;
        }
        else
        {
            try
            {
                result = ToType<T>(value);
            }
            catch (Exception)
            {
                result = defaultValue;
            }
        }
        return result;
    }

    private static Object? ToType(String value, Type conversionType)
    {
        Object? result;
        if (conversionType == typeof(String))
        {
            result = value;
        }
        else if (conversionType == typeof(Int32))
        {
            result = ((value == null) ? 0 : Int32.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(Boolean))
        {
            result = value.ToDGBool();
        }
        else if (conversionType == typeof(Single))
        {
            result = ((value == null) ? 0f : Single.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(Double))
        {
            result = ((value == null) ? 0.0 : Double.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(Decimal))
        {
            result = ((value == null) ? 0m : Decimal.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(DateTime))
        {
            result = ((value == null) ? DateTimeUtil.MinValue : DateTime.Parse(value, CultureInfo.CurrentCulture, DateTimeStyles.None));
        }
        else if (conversionType == typeof(Char))
        {
            result = Convert.ToChar(value);
        }
        else if (conversionType == typeof(SByte))
        {
            result = SByte.Parse(value, NumberStyles.Any);
        }
        else if (conversionType == typeof(Byte))
        {
            result = Byte.Parse(value, NumberStyles.Any);
        }
        else if (conversionType == typeof(Int16))
        {
            result = (Int32)((value == null) ? 0 : Int16.Parse(value));
        }
        else if (conversionType == typeof(UInt16))
        {
            result = (Int32)((value == null) ? 0 : UInt16.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(UInt32))
        {
            result = ((value == null) ? 0U : UInt32.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(Int64))
        {
            result = ((value == null) ? 0L : Int64.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(UInt64))
        {
            result = ((value == null) ? 0UL : UInt64.Parse(value, NumberStyles.Any));
        }
        else if (conversionType == typeof(Guid))
        {
            result = ((value == null) ? Guid.Empty : new Guid(value));
        }
        else
        {
            result = null;
        }
        return result;
    }

}