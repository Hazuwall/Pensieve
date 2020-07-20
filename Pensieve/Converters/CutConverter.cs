using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Data;

namespace Pensieve
{
    /// <summary>
    /// Конвертер для обрезки части строки
    /// </summary>
    public class CutConverter : IValueConverter
    {
        public int Leading { get; set; }
        public int Trailing { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = value as string;
            return str.Substring(this.Leading, str.Length - this.Trailing - this.Leading);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
