using System;

namespace Loader
{
    public static class Extensions
    {
        /// <summary>
        /// Дата без секунд, милисекунд, тиков и т.д.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime RecreateDate(this DateTime date) => new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);

        /// <summary>
        /// Обрезка строки между строками
        /// </summary>
        /// <param name="sourceString">Исходная строка</param>
        /// <param name="startString">Начало строки(усекается)</param>
        /// <param name="endString">Конец строки</param>
        /// <param name="includeLastSymbol">Включать конец строки в результат</param>
        /// <returns></returns>
        public static string Cut(this string sourceString, string startString = null, string endString = null, bool includeLastSymbol = false)
        {
            var stIndex = startString == null ? 0 : sourceString.IndexOf(startString) + startString.Length;
            var endIndex = endString == null ? 0 : sourceString.IndexOf(endString) + (includeLastSymbol ? endString.Length : 0);

            return stIndex == -1 || endIndex == -1
                ? sourceString
                : sourceString.Substring(stIndex, endIndex - stIndex);
        }
    }
}
