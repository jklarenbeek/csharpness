using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace joham.cs_futils
{
    public static class TextHelper
    {
        public static int EvalInt(object right)
        {
            if (right == null)
                return 0;
            if (right == DBNull.Value)
                return 0;
            if (right is bool)
                return ((bool)right) ? 1 : 0;
            if (right is int)
                return (int)right;
            if (right is decimal)
                return (int)((decimal)right);
            if (right is float)
                return (int)((float)right);
            if (right is double)
                return (int)((double)right);

            throw new NotImplementedException(string.Format("TextRenderer.EvalInt(object right): ERROR right is of type {0}", right.GetType()));

        }
        public static double EvalDouble(object right)
        {
            if (right == null)
                return 0;
            if (right == DBNull.Value)
                return 0;
            if (right is bool)
                return ((bool)right) ? 1 : 0;
            if (right is int)
                return (double)((int)right);
            if (right is decimal)
                return (double)((decimal)right);
            if (right is float)
                return (double)((float)right);
            if (right is double)
                return (double)((double)right);

            throw new NotImplementedException(string.Format("TextRenderer.EvalDouble(object right): ERROR right is of type {0}", right.GetType()));
        }
        public static double EvalMin(object left, object right)
        {
            double dl = EvalDouble(left);
            double dr = EvalDouble(right);

            return Math.Min(dl, dr);
        }
        public static double EvalSub(object left, object right)
        {
            double dl = EvalDouble(left);
            double dr = EvalDouble(right);

            return dl - dr;
        }
        public static double EvalAdd(object left, object right)
        {
            double dl = EvalDouble(left);
            double dr = EvalDouble(right);

            return dl + dr;
        }
        public static string FormatInt(object right)
        {
            if (right == null)
                return "";
            if (right == DBNull.Value)
                return "";
            if (right is int)
                return ((int)right).ToString();
            if (right is decimal)
                return Convert.ToInt32(right).ToString();
            if (right is float)
                return Convert.ToInt32(right).ToString();
            if (right is double)
                return Convert.ToInt32(right).ToString();
            if (right is string)
                return Convert.ToInt32(right).ToString();

            throw new NotImplementedException(string.Format("TextRenderer.FormatInt:Cant convert {0} to int to string", right.GetType()));
        }
        public static string FormatDouble(object right)
        {
            if (right == null)
                return "";
            if (right == DBNull.Value)
                return "";
            if (right is int)
                return Convert.ToDouble(right).ToString();
            if (right is decimal)
                return Convert.ToDouble(right).ToString();
            if (right is float)
                return Convert.ToDouble(right).ToString();
            if (right is double)
            {
                double r = (double)right;
                if (r >= Int32.MaxValue || r <= Int32.MinValue)
                    return "";
                else
                    return (r).ToString();
            }
            if (right is string)
                return Convert.ToDouble(right).ToString();

            throw new NotImplementedException(string.Format("TextRenderer.FormatDouble:Cant convert {0} to double to string", right.GetType()));
        }
        public static string FormatPercentage(object val)
        {
            string v = FormatInt(val);
            return (String.IsNullOrWhiteSpace(v)) ? "" : string.Format("{0}%", v);
        }
        public static string FormatAmount(object right, int decimals)
        {
            if (right == null)
                return "";
            if (right == DBNull.Value)
                return "";
            double val = EvalDouble(right);
            if (val == 0)
                return "";
            return val.ToString(string.Format("C{0}", decimals), System.Globalization.CultureInfo.CurrentCulture);
        }
        public static string FormatAmount(object right)
        {
            return FormatAmount(right, 0);
        }
        public static string FormatHours(object val)
        {
            string v = FormatInt(val);
            return (String.IsNullOrWhiteSpace(v) || v == "0") ? "" : string.Format("{0} uur", v);
        }
        public static string FormatString(object val, bool replaceQoute)
        {
            if (val == null)
                return "";
            if (val == DBNull.Value)
                return "";
            string ret = val.ToString();

            return ret.Replace('\"', '\'');
        }
        public static string FormatShortDateFromDateTime(object obj)
        {
            if (obj == null)
                return null;
            if (obj == DBNull.Value)
                return null;
            if (obj is DateTime)
                return ((DateTime)obj).ToString("yyyy-MM-dd");
            if (obj is string)
            {
                DateTime result;
                if (DateTime.TryParse(obj as string, out result))
                    return result.ToString("yyyy-MM-dd");
            }
                
            return null;
        }
        public static string FormatMonthDayFromDateTime(object obj)
        {
            if (obj != null && obj != DBNull.Value)
                return ((DateTime)obj).ToString("dd MMM");

            return null;
        }
        public static string FormatYearMonthDayFromDateTime(object obj)
        {
            if (obj != null && obj != DBNull.Value)
                return ((DateTime)obj).ToString("dd MMM yy");

            return null;
        }
        public static string FormatDayMonthFromDateTime(object obj)
        {
            if (obj != null && obj != DBNull.Value)
                return ((DateTime)obj).ToString("d MMMM");

            return null;

        }

        public static string EnsureEndWithSemiColon(string value)
        {
            if (value != null)
            {
                int length = value.Length;
                if (length > 0 && value[length - 1] != ';')
                {
                    return (value + ";");
                }
            }
            return value;
        }

    }
}

