using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pensieve
{
    public class Database : DatabaseBase
    {
        public struct SearchParams
        {
            public readonly bool ImportantOnly;
            public readonly List<string> Tags;
            public readonly string SearchString;

            public SearchParams(bool ImportantOnly, List<string> Tags, string SearchString)
            {
                this.ImportantOnly = ImportantOnly;
                this.Tags = Tags;
                this.SearchString = SearchString;
            }
        }

        public const string DateFormat = "yyyy.MM.dd";

        private static Database _Current;
        public static Database Current {
            get
            {
                if (_Current == null)
                    _Current = new Database();
                return _Current;
            }
        }

        private Database()
            : base("Diary.db", new string[] { "docs", "notes", "notes_docs", "notes_songs", "notes_tags", "songs", "tags", "years" })
        {
        }

        #region Записи
        public bool AddNote(Note Note)
        {
            bool result;
            this.BeginTransaction();
            using (var query = con.Prepare("INSERT OR ROLLBACK INTO notes VALUES (" + Note.ID + ",'" + Note.Date.ToString(DateFormat) + "'," + ((Note.IsImportant) ? 1 : 0) + ",'" + Note.Title.Shielding() + "','" + Note.Text.Shielding() + "','" + Note.Extra.Shielding() + "')"))
                result = query.Step() == SQLiteResult.DONE;
            SetNoteResources(ResourceType.Tags, Note.ID, Note.Tags);
            SetNoteResources(ResourceType.Songs, Note.ID, Note.Songs);
            SetNoteResources(ResourceType.Docs, Note.ID, Note.Docs);
            this.EndTransaction();
            return result;
        }
        public bool UpdateNote(long ID, Note Note)
        {
            bool result;
            this.BeginTransaction();
            if (ID != Note.ID)
            {
                this.SetNoteResources(ResourceType.Tags, ID, null);
                this.SetNoteResources(ResourceType.Songs, ID, null);
                this.SetNoteResources(ResourceType.Docs, ID, null);
            }
            using (var query = con.Prepare("UPDATE OR ROLLBACK notes SET id= " + Note.ID + ", date='" + Note.Date.ToString(DateFormat) + "', important=" + ((Note.IsImportant) ? 1 : 0) + ", title='" + Note.Title.Shielding() + "', text='" + Note.Text.Shielding() + "', extra='" + Note.Extra.Shielding() + "' WHERE id=" + ID))
                result = query.Step() == SQLiteResult.DONE;
            this.SetNoteResources(ResourceType.Tags, Note.ID, Note.Tags);
            this.SetNoteResources(ResourceType.Songs, Note.ID, Note.Songs);
            this.SetNoteResources(ResourceType.Docs, Note.ID, Note.Docs);
            this.EndTransaction();
            return result;
        }
        public Note GetNote(long ID)
        {
            using (var query = con.Prepare("SELECT date, important, title, text, extra FROM notes WHERE id=" + ID))
            {
                if (query.Step() != SQLiteResult.ROW)
                    return null;
                Note note = new Note();
                note.Date = DateTime.Parse((string)query[0]);
                note.IsImportant = (long)query[1] == 1;
                note.Title = (string)query[2];
                note.Text = (string)query[3];
                note.Extra = (string)query[4];
                note.Tags = GetNoteResources(ResourceType.Tags, note.ID);
                note.Songs = GetNoteResources(ResourceType.Songs, note.ID);
                note.Docs = GetNoteResources(ResourceType.Docs, note.ID);
                return note;
            }
        }
        public void DeleteNote(long ID)
        {
            using (var query = con.Prepare("DELETE FROM notes WHERE id=" + ID))
                query.Step();
        }
        #endregion

        #region Выбор дня
        public List<YearInfo> GetNoteYears(SearchParams Params)
        {
            List<YearInfo> result = new List<YearInfo>();
            string cond = PrepareSearchNotesCondition("notes.id>0",Params);
            using (var query = con.Prepare(String.Format("SELECT DISTINCT CAST(SUBSTR(date,0,5) AS INTEGER) FROM notes {0} ORDER BY notes.id ASC", cond)))
                while(query.Step()==SQLiteResult.ROW)
                    result.Add(GetYearInfo((int)((long)query[0])));
            return result;
        }
        public List<int> GetNoteMonths(SearchParams Params, int Year)
        {
            DateTime startDate = new DateTime(Year,1,1);
            string IDBounds = "notes.id BETWEEN " + (startDate.Ticks/TimeSpan.TicksPerDay) + " AND " + (startDate.AddYears(1).Ticks/TimeSpan.TicksPerDay);
            List<int> result = new List<int>();
            string cond = PrepareSearchNotesCondition(IDBounds,Params);
            using (var query = con.Prepare(String.Format("SELECT DISTINCT CAST(SUBSTR(date,6,2) AS INTEGER) FROM notes {0} ORDER BY notes.id ASC", cond)))
                while(query.Step()==SQLiteResult.ROW)
                    result.Add((int)((long)query[0]));
            return result;
        }
        public List<DayInfo> GetNoteDays(SearchParams Params, int Year, int Month)
        {
            DateTime startDate = new DateTime(Year, Month, 1);
            string IDBounds = "notes.id BETWEEN " + (startDate.Ticks / TimeSpan.TicksPerDay) + " AND " + ((startDate.AddMonths(1).Ticks / TimeSpan.TicksPerDay) - 1); //выборка включает пределы
            List<DayInfo> result = new List<DayInfo>();
            string cond = PrepareSearchNotesCondition(IDBounds, Params);
            using (var query = con.Prepare(String.Format("SELECT DISTINCT CAST(SUBSTR(date,9,2) AS INTEGER), title, important FROM notes {0} ORDER BY notes.id ASC", cond)))
                while (query.Step() == SQLiteResult.ROW)
                    result.Add(new DayInfo((int)((long)query[0]), (string)query[1], (long)query[2] == 1));
            return result;
        }
        private string PrepareSearchNotesCondition(string IDBounds, SearchParams Params)
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("WHERE {0}",IDBounds);
            if (!String.IsNullOrWhiteSpace(Params.SearchString))
                str.AppendFormat(" AND (notes.title LIKE '%{0}%' OR notes.text LIKE '%{0}%' OR notes.id IN (SELECT badges.note_id FROM badges WHERE badges.project_title LIKE '%{0}%' OR badges.name LIKE '%{0}%'))", Params.SearchString);
            if (Params.ImportantOnly)
                str.Append(" AND notes.important=1");
            if (Params.Tags != null && Params.Tags.Count > 0)
            {
                long[] tagIDs = new long[Params.Tags.Count];
                for (int i = 0; i < Params.Tags.Count; i++)
                    tagIDs[i]=this.GetResourceID(ResourceType.Tags, Params.Tags[i]);
                str.AppendFormat(" AND notes.id IN (SELECT notes_tags.note_id FROM notes_tags WHERE notes_tags.tag_id IN ({0}))", tagIDs.ToPlainList());
            }
            return str.ToString();
        }
        #endregion

        #region Описание года
        /// <summary>
        /// Добавить или изменить информацию о годе
        /// </summary>
        /// <param name="YearInfo"></param>
        public void SetYearInfo(YearInfo YearInfo)
        {
            using (var query = con.Prepare("INSERT OR REPLACE INTO years(year, brief, color) VALUES (" + YearInfo.Number + ",'" + YearInfo.Brief.Shielding() + "','" + YearInfo.ColorCode + "')"))
                query.Step();
        }
        /// <summary>
        /// Получить информацию о годе
        /// </summary>
        /// <param name="Year">Номер года</param>
        /// <returns></returns>
        public YearInfo GetYearInfo(int Year)
        {
            using (var query = con.Prepare("SELECT brief, color FROM years WHERE year=" + Year))
            {
                if (query.Step() == SQLiteResult.ROW)
                    return new YearInfo(Year, (string)query[0], (string)query[1]);
                else
                    return new YearInfo(Year, String.Empty, "#000000");
            }
        }
        #endregion

        #region Ресурсы
        public bool AddResource(ResourceType Table, string Name)
        {
            using (var query = con.Prepare(String.Format("INSERT INTO {0} (name) VALUES ('{1}')", Table.GetTableName(), Name.Shielding())))
                return query.Step() == SQLiteResult.DONE;
        }
        public bool UpdateResource(ResourceType Table, string OldName, string NewName)
        {
            using (var query = con.Prepare(String.Format("UPDATE {0} SET name='{1}' WHERE name='{2}'", Table.GetTableName(), NewName.Shielding(), OldName.Shielding())))
                return query.Step() == SQLiteResult.DONE;
        }
        public long GetResourceID(ResourceType Table, string Name)
        {
            using (var query = con.Prepare(String.Format("SELECT id FROM {0} WHERE name = '{1}'", Table.GetTableName(), Name.Shielding())))
                return query.Step() == SQLiteResult.ROW ? (long)query[0] : 0;
        }
        public List<string> GetResources(ResourceType Table)
        {
            List<string> result = new List<string>();
            string tableName = Table.GetTableName();
            using (var query = con.Prepare(String.Format("SELECT name FROM {0} ORDER BY name ASC", tableName)))
                while (query.Step() == SQLiteResult.ROW)
                    result.Add((string)query[0]);
            return result;
        }
        public bool DeleteResource(ResourceType Table, string Name)
        {
            using (var query = con.Prepare(String.Format("DELETE FROM {0} WHERE name='{1}'", Table.GetTableName(), Name.Shielding())))
                return query.Step() == SQLiteResult.DONE;
        }

        public List<string> GetNoteResources(ResourceType Table, long NoteID)
        {
            List<string> result = new List<string>();
            string tableName = Table.GetTableName();
            string itemName = tableName.Substring(0, tableName.Length - 1);
            using (var query = con.Prepare(String.Format("SELECT name FROM {0} WHERE id IN (SELECT {1}_id FROM notes_{0} WHERE note_id={2})", tableName, itemName, NoteID)))
                while (query.Step() == SQLiteResult.ROW)
                    result.Add((string)query[0]);
            return result;
        }
        public void SetNoteResources(ResourceType Table, long NoteID, List<string> Resources)
        {
            string tableName = Table.GetTableName();
            string itemName = tableName.Substring(0, tableName.Length - 1);
            using (var query = con.Prepare(String.Format("DELETE FROM notes_{0} WHERE note_id={1}", tableName, NoteID)))
                query.Step();
            if(Resources!=null)
                foreach (string item in Resources)
                    using (var query = con.Prepare(String.Format("INSERT INTO notes_{0} (note_id,{1}_id) VALUES ({2}, {3})", tableName, itemName, NoteID, GetResourceID(Table,item))))
                        query.Step();
        }
        #endregion
    }
}
