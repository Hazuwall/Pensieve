using System;
using System.Collections.Generic;

namespace Pensieve
{
    /// <summary>
    /// Запись дневника
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long ID
        {
            get { return this.Date.Ticks / TimeSpan.TicksPerDay; }
        }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Заголовок
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Содержание записи
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Ключевые слова
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// Музыка, имеющая отношение к записи
        /// </summary>
        public List<string> Songs { get; set; }
        /// <summary>
        /// Документы, имеющие отношение к записи
        /// </summary>
        public List<string> Docs { get; set; }
        /// <summary>
        /// Важна ли запись
        /// </summary>
        public bool IsImportant { get; set; }
        /// <summary>
        /// Другая информация
        /// </summary>
        public string Extra { get; set; }


        public Note()
        {
            this.Date = DateTime.Now;
            this.IsImportant = false;
            this.Title = String.Empty;
            this.Text = String.Empty;
            this.Extra = String.Empty;
        }
    }
}
