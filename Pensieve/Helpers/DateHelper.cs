using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pensieve
{
    public static class DateHelper
    {
        private static string[] _MonthNames = new string[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

        /// <summary>
        /// Получить название месяца
        /// </summary>
        /// <param name="Num">Номер месяца 1-12</param>
        /// <param name="DoStartWithCap">Начинать ли с заглавной буквы</param>
        /// <param name="IsGenitive">Использовать ли родительный падеж</param>
        /// <returns>Название месяца</returns>
        public static string GetMonthName(int Num, bool DoStartWithCap, bool IsGenitive)
        {
            string result = _MonthNames[Num - 1];
            if (IsGenitive)
            {
                if (Num == 3 || Num == 8)
                    result = result + "а";
                else
                    result = result.Substring(0, result.Length - 1) + "я";
            }
            return DoStartWithCap ? result : result.ToLower();
        }

        public static string GetDateString(DateTime date) {
            return date.Day + " " + GetMonthName(date.Month, false, true) + " " + date.Year;
        }

        /// <summary>
        /// Получить индекс дня недели (1-7)
        /// </summary>
        /// <returns></returns>
        public static int GetDayOfWeekIndex(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 7;
                default:
                    return 1;
            }
        }
    }
}
