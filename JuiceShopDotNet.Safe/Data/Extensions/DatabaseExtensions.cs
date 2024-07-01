using Microsoft.Data.SqlClient;

namespace JuiceShopDotNet.Safe.Data.Extensions;

public static class DatabaseExtensions
{
    public static object ToDBNullable(this string value)
    {
        if (value == null)
            return DBNull.Value;
        else
            return value;
    }

    public static DateTime? GetNullableDateTime(this SqlDataReader reader, int index)
    {
        var value = reader.GetValue(index);

        if (value == DBNull.Value)
            return null;
        else
            return Convert.ToDateTime(value);
    }

    public static string? GetNullableString(this SqlDataReader reader, int index)
    {
        var value = reader.GetValue(index);

        if (value == DBNull.Value)
            return null;
        else
            return value.ToString();
    }
}
