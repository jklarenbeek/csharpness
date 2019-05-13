using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace joham.cs_futils.data {

  public static class DataHelper {
          public static int? ConvertStringToDBInt(string input)
        {
            int tmp;
            if (String.IsNullOrWhiteSpace(input)
                || Int32.TryParse(
                        input,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture,
                        out tmp) == false)
                return null;

            return tmp;
        }
        public static Dictionary<string, object> ConvertToDictionary(DataRow row)
        {
            if (row == null)
                return null;

            Dictionary<string, object> result = row.Table.Columns.Cast<DataColumn>()
                .ToDictionary(
                    col => col.ColumnName,
                    col => row.Field<object>(col.ColumnName)
                );

            return result;
        }
        public static Dictionary<string, object> ConvertToDictionary(DataTable table)
        {
            DataRow row = table.AsEnumerable().FirstOrDefault();
            if (row == null)
                return ConvertToDictionary(table.NewRow());
            else
                return ConvertToDictionary(row);
        }
        public static Dictionary<string, string> ConvertToStringDictionary(Dictionary<string, object> row)
        {
            if (row == null)
                return null;

            return row.ToDictionary(k => k.Key, k => k.Value.ToString());
        }
        public static Dictionary<string, string> ConvertToStringDictionary(DataRow row)
        {
            // TODO: preferably this whole helper function is removed infavor of ConvertToDictionary
            if (row == null)
                return null;

            Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                .ToDictionary(
                    col => col.ColumnName,
                    col => Convert.ToString(row.Field<object>(col.ColumnName))
                );

            return result;

        }
        public static Dictionary<string, string> ConvertToStringDictionary(DataTable table)
        {
            return ConvertToStringDictionary(table.AsEnumerable().FirstOrDefault());
        }

  }
}