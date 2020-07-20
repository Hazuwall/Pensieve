using System;
using System.Collections.Generic;
using System.Text;

namespace Pensieve
{
    /// <summary>
    /// Уникальная строка. Имеет идентификатор
    /// </summary>
    public struct UniqueString
    {
        /// <summary>
        /// Пустая строка
        /// </summary>
        public static readonly UniqueString Empty = new UniqueString(0, string.Empty);
        public int ID;
        public string Value;
        public UniqueString (int ID, string Value)
        {
            this.ID = ID;
            this.Value = Value;
        }
        public override string ToString()
        {
            return this.Value;
        }
    }
}
