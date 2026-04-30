using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Repository
{
    public static class DataTypeHelper
    {
        public static string GetString(object value) =>
    value == DBNull.Value || value == null ? string.Empty : value.ToString();

        public static int GetInt(object value) =>
            value == DBNull.Value || value == null ? 0 : Convert.ToInt32(value);

        public static long GetLong(object value) =>
            value == DBNull.Value || value == null ? 0L : Convert.ToInt64(value);

        public static short GetShort(object value) =>
            value == DBNull.Value || value == null ? (short)0 : Convert.ToInt16(value);

        public static decimal GetDecimal(object value) =>
            value == DBNull.Value || value == null ? 0m : Convert.ToDecimal(value);

        public static double GetDouble(object value) =>
            value == DBNull.Value || value == null ? 0d : Convert.ToDouble(value);

        public static float GetFloat(object value) =>
            value == DBNull.Value || value == null ? 0f : Convert.ToSingle(value);

        public static bool GetBool(object value) =>
            value != DBNull.Value && value != null && Convert.ToBoolean(value);

        public static DateTime GetDateTime(object value) =>
            value == DBNull.Value || value == null ? DateTime.MinValue : Convert.ToDateTime(value);

        public static Guid GetGuid(object value) =>
            value == DBNull.Value || value == null ? Guid.Empty : Guid.Parse(value.ToString());

        public static byte[] GetBytes(object value) =>
            value == DBNull.Value || value == null ? new byte[0] : (byte[])value;
        public static int? GetNullableInt(object value) =>
    value == DBNull.Value || value == null ? (int?)null : Convert.ToInt32(value);

        public static decimal? GetNullableDecimal(object value) =>
            value == DBNull.Value || value == null ? (decimal?)null : Convert.ToDecimal(value);

        public static DateTime? GetNullableDateTime(object value) =>
            value == DBNull.Value || value == null ? (DateTime?)null : Convert.ToDateTime(value);

        public static bool? GetNullableBool(object value) =>
            value == DBNull.Value || value == null ? (bool?)null : Convert.ToBoolean(value);
    }
}
