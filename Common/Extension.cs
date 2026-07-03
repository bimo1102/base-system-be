using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Common;

public static class Extension
{
    #region Date Extension

    public static DateTime GetCurrentDate()
    {
        return DateTime.Now;
    }

    public static DateTime GetCurrentDateUtc()
    {
        return DateTime.UtcNow;
    }

    public static long AsUnixTimeStamp(this DateTime item)
    {
        try
        {
            return (long)item.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
        catch (Exception)
        {
            return (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }

    #endregion

    #region Enum Extension

    public static string GetDisplayName(this Enum? enumValue)
    {
        try
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            var configName = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();
            return string.IsNullOrEmpty(configName) ? enumValue.ToString() : configName;
        }
        catch (Exception e)
        {
            return enumValue.AsEmptyString();
        }
    }

    #endregion

    // transform object into string data type

    public static string AsEmptyString(this string? item)
    {
        return item == null ? string.Empty : item.Trim();
    }

    public static string AsEmptyString(this object? item)
    {
        return item switch
        {
            null => string.Empty,
            string str => str.Trim(),
            _ => item.ToString()?.Trim() ?? string.Empty
        };
    }

    public static string AsArrayJoin(this string[]? strings)
    {
        return strings?.Length > 0 ? string.Join(",", strings) : string.Empty;
    }
}