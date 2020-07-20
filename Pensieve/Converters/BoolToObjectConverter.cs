using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Pensieve
{
    /// <summary>
    /// Конвертер-переключатель
    /// </summary>
    public class BoolToObjectConverter : IValueConverter
    {
        public object IfFalse { get; set; }
        public object IfTrue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? this.IfTrue : this.IfFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(this.IfTrue);
        }
    }
}
