using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace Pensieve
{
    public class DatabaseBase
    {
        protected readonly SQLiteConnection con;
        protected readonly string[] Tables;
        protected readonly Uri AppxUri;

        public readonly string FileName;
        
        /// <summary>
        /// Инициализировать новое подключение к локальной базе данных
        /// </summary>
        /// <param name="fileName">Название файла в локальном хранилище</param>
        /// <param name="tables">Таблицы, содержащиеся в базе данных</param>
        /// <param name="access">Режим доступа</param>
        /// <param name="appxUri">Ссылка на исходную версию базы данных</param>
        public DatabaseBase(string fileName, string[] tables, SQLiteOpen access, Uri appxUri, bool doSetDefault = false)
        {
            this.FileName = fileName;
            this.Tables = tables;
            this.AppxUri = appxUri;
            if(doSetDefault)
                AsyncHelper.RunSync(this.SetDefaultAsync);
            this.con = new SQLiteConnection(fileName,access);
            
            this.ActivateForeignKeys();
        }
        /// <summary>
        /// Инициализировать новое подключение к локальной базе данных
        /// </summary>
        /// <param name="fileName">Название файла в локальном хранилище</param>
        /// <param name="tables">Таблицы, содержащиеся в базе данных</param>
        /// <param name="access">Режим доступа</param>
        public DatabaseBase(string fileName, string[] tables, SQLiteOpen access)
            :this(fileName,tables,access, new Uri("ms-appx:///Data/" + fileName))
        {
        }
        /// <summary>
        /// Инициализировать новое подключение к локальной базе данных
        /// </summary>
        /// <param name="fileName">Название файла в локальном хранилище</param>
        /// <param name="tables">Таблицы, содержащиеся в базе данных</param>
        public DatabaseBase(string fileName, string[] tables)
            : this(fileName, tables, SQLiteOpen.READWRITE, new Uri("ms-appx:///Data/" + fileName))
        {
        }

        /// <summary>
        /// Подготовить SQL запрос к базе данных
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public ISQLiteStatement Prepare(string sql)
        {
            return con.Prepare(sql);
        }

        /// <summary>
        /// Начать транзакцию
        /// </summary>
        /// <returns>Выполнено</returns>
        public bool BeginTransaction()
        {
            using (var query = con.Prepare("BEGIN TRANSACTION"))
                return query.Step() == SQLiteResult.DONE;
        }

        /// <summary>
        /// Закончить транзакцию
        /// </summary>
        /// <returns>Выполнено</returns>
        public bool EndTransaction()
        {
            using (var query = con.Prepare("END TRANSACTION"))
                return query.Step() == SQLiteResult.DONE;
        }

        /// <summary>
        /// Откатить транзакцию
        /// </summary>
        /// <returns>Выполнено</returns>
        public bool Rollback()
        {
            using (var query = con.Prepare("ROLLBACK TRANSACTION"))
                return query.Step() == SQLiteResult.DONE;
        }

        /// <summary>
        /// Вернуть базу данных в исходное состояние. Скопировать с заменой базу данных из установочного пакета
        /// </summary>
        /// <returns></returns>
        public virtual async Task SetDefaultAsync()
        {
            StorageFile db = await StorageFile.GetFileFromApplicationUriAsync(this.AppxUri);
            await db.CopyAsync(ApplicationData.Current.LocalFolder, this.FileName, NameCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// Включить внешние ключи в базе данных
        /// </summary>
        public void ActivateForeignKeys()
        {
            using (var query = con.Prepare("PRAGMA foreign_keys=on"))
                query.Step();
        }

        /// <summary>
        /// Является ли файл корректной базой данных этого типа. Осуществляется проверка на существование всех таблиц
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsValidAsync(StorageFile file)
        {
            try
            {
                StorageFile tempFile = await file.CopyAsync(ApplicationData.Current.LocalFolder, "Temp.db", NameCollisionOption.ReplaceExisting);
                bool IsValid = true;
                using (SQLiteConnection tempCon = new SQLiteConnection("Temp.db"))
                {
                    List<string> tableNames = new List<string>();
                    using (var query = tempCon.Prepare(@"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name ASC"))
                        while (query.Step() == SQLiteResult.ROW)
                            tableNames.Add((string)query[0]);
                    //Все ли требуемые таблицы содержатся в рассматриваемой базе данных
                    foreach (var name in this.Tables)
                        IsValid &= tableNames.Contains(name);
                }
                await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return IsValid;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Импортировать базу данных
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ImportAsync()
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.FileTypeFilter.Add(".db");
                picker.CommitButtonText = "Выбрать";
                StorageFile DBFile = await picker.PickSingleFileAsync();
                if (DBFile != null && await this.IsValidAsync(DBFile))
                {
                    await DBFile.CopyAsync(ApplicationData.Current.LocalFolder, this.FileName, NameCollisionOption.ReplaceExisting);
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Экспортировать локальную базу данных в выбранную пользователем директорию
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ExportAsync()
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileSavePicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                picker.CommitButtonText = "Сохранить";
                picker.DefaultFileExtension = ".db";
                picker.FileTypeChoices.Add("База данных", new List<string>() { ".db" });
                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    await (await ApplicationData.Current.LocalFolder.GetFileAsync(this.FileName)).CopyAndReplaceAsync(file);
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
