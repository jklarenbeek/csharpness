using System;
using System.Globalization;

namespace joham.cs_futils
{
    public static class DateTimeExtensions
    {
        public static int IsoWeek(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static int IsoYear(this DateTime time)
        {
            int week = IsoWeek(time);
            if (time.Month == 12 && week == 1)
                return time.Year + 1;

            if (time.Month == 1 && week > 51)
                return time.Year - 1;

            return time.Year;
        }
    }

}