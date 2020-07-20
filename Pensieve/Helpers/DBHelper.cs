namespace Pensieve
{
    public static class DBHelper
    {
        /// <summary>
        /// Подготовить строку для записи в базу данных
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Shielding(this string str)
        {
            return str.Trim().Replace("'", "''");
        }
    }
}
