using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pensieve
{
    public delegate void TextEditedEventHandler(object sender, TextEditedEventArgs e);

    public class TextEditedEventArgs
    {
        private string _NewValue;
        private string _OldValue;

        /// <summary>
        /// Корректно ли новое значение
        /// </summary>
        public bool IsValid { get; set; }
        public string NewValue { get { return this._NewValue; } }
        public string OldValue { get { return this._OldValue; } }

        public TextEditedEventArgs(string OldValue, string NewValue)
        {
            this.IsValid = true;
            this._OldValue = OldValue;
            this._NewValue = NewValue;
        }
    }
}
