using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Pensieve
{
    public static class FormatHelper
    {
        /// <summary>
        /// Представить одномерный массив в виде списка элементов, разделённых запятой
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static string ToPlainList(this Array arr)
        {
            if (arr.Length > 0)
            {
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    str.Append(arr.GetValue(i));
                    str.Append(", ");
                }
                str.Append(arr.GetValue(arr.Length - 1));
                return str.ToString();
            }
            else
                return String.Empty;
        }

        /// <summary>
        /// Начать строку с заглавной буквы
        /// </summary>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FirstLetterToUpperCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("There is no first letter");
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
