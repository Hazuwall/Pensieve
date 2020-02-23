using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Hazuwall.Tools;
using Hazuwall;


namespace Pensieve
{
    public static class Library
    {
        private static readonly StorageFolder Root = ApplicationData.Current.LocalFolder;

        #region Библиотека
        /// <summary>
        /// Создать системные директории в корневой папке
        /// </summary>
        /// <returns></returns>
        public static async Task CreateAsync()
        {
            await Root.CreateFolderAsync("Songs", CreationCollisionOption.OpenIfExists);
            await Root.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
            await Root.CreateFolderAsync("Docs", CreationCollisionOption.OpenIfExists);
        }
        /// <summary>
        /// Импортировать
        /// </summary>
        /// <returns>Успешность выполнения</returns>
        public static async Task<bool> ImportAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(".zip");
            picker.CommitButtonText = "Выбрать";
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await file.UnzipArchiveAsync(Root, CreationCollisionOption.ReplaceExisting, new string[] { "Images", "Songs", "Docs" });
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Экспортировать
        /// </summary>
        /// <param name="DoIncludeMusic">Включить музыку в архив?</param>
        /// <returns>Успешность выполнения</returns>
        public static async Task<bool> ExportAsync(bool DoIncludeMusic)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.CommitButtonText = "Сохранить";
            picker.DefaultFileExtension = ".zip";
            picker.FileTypeChoices.Add("Zip-Архив",new List<string>(){".zip"});
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    List<IStorageItem> list = new List<IStorageItem>();
                    list.Add(await Root.GetFolderAsync("Images"));
                    list.Add(await Root.GetFolderAsync("Docs"));
                    if (DoIncludeMusic)
                        list.Add(await Root.GetFolderAsync("Songs"));
                    await StorageHelper.CreateArchiveAsync(file, list, 9);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region Картинки
        /// <summary>
        /// Получить файлы изображений записи
        /// </summary>
        /// <param name="ID">ID записи</param>
        /// <returns></returns>
        public static async Task<List<StorageFile>> GetImagesAsync(long ID)
        {
            DateTime Date = new DateTime(ID * TimeSpan.TicksPerDay);
            List<StorageFile> list = new List<StorageFile>();
            try
            {
                StorageFolder dir = await StorageFolder.GetFolderFromPathAsync(String.Format(@"{0}\Images\{1:D4}\{2:D2}\{3:D2}", Root.Path, Date.Year, Date.Month, Date.Day));
                foreach (var file in await dir.GetFilesAsync())
                    list.Add(file);
            }
            catch { }
            return list;
        }
        /// <summary>
        /// Установить изображения для записи
        /// </summary>
        /// <param name="ID">ID записи</param>
        /// <param name="Files">Словарь файлов с названием картинок в качестве ключей</param>
        /// <returns></returns>
        public static async Task SetImagesAsync(long ID, Dictionary<string, StorageFile> Files)
        {
            DateTime Date = new DateTime(ID * TimeSpan.TicksPerDay);
            string localPath = String.Format(@"Images\{0:D4}\{1:D2}\{2:D2}", Date.Year, Date.Month, Date.Day);
            StorageFolder folder = await StorageHelper.TryGetLocalFolderAsync(localPath);
            bool isEmptySet = Files == null || Files.Count == 0;

            //Подготовка директории фото
            if (folder != null)
            {
                if (isEmptySet)
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                else
                    foreach (IStorageItem item in await folder.GetItemsAsync())
                        await item.DeleteAsync();
            }
            else
            {
                folder = await StorageHelper.CreateLocalFolderAsync(localPath);
            }

            //Помещение фотографий в директорию
            if (!isEmptySet) 
                foreach (var file in Files)
                    await file.Value.CopyAsync(folder, file.Key);
        }
        #endregion

        #region Ресурсы
        /// <summary>
        /// Выбрать ресурсы и добавить их в библиотеку с заменой при совпадении имён
        /// </summary>
        /// <param name="type">Тип ресурсов, которые нужно добавить</param>
        /// <exception cref="System.ArgumentException"/>
        /// <returns>Были ли выбраны файлы</returns>
        public static async Task<List<StorageFile>> PickAndAddResourcesAsync(ResourceType type)
        {
            if(type == ResourceType.Tags)
                throw new ArgumentException("Ключевые слова не хранятся в виде файлов библиотеки");

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(type==ResourceType.Songs? ".mp3" : "*");
            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();
            List<StorageFile> result = new List<StorageFile>();
            if (files.Count > 0)
            {
                StorageFolder dir = await Root.GetFolderAsync(type.GetTableName().FirstLetterToUpperCase());
                foreach (var file in files)
                    result.Add(await file.CopyAsync(dir, file.Name, NameCollisionOption.ReplaceExisting));
            }
            return result;
        }

        /// <summary>
        /// Получить ресурс из библиотеки
        /// </summary>
        /// <param name="type">Тип ресурса</param>
        /// <param name="name">Название с расширением</param>
        /// <exception cref="System.ArgumentException"/>
        /// <returns></returns>
        public static Task<StorageFile> GetResourceAsync(ResourceType type, string name)
        {
            if (type == ResourceType.Tags)
                throw new ArgumentException("Ключевые слова не хранятся в виде файлов библиотеки");

            string path = Root.Path + "\\" + type.GetTableName().FirstLetterToUpperCase() + "\\" + name;
            return StorageFile.GetFileFromPathAsync(path).AsTask();
        }

        /// <summary>
        /// Получить список всех ресурсов выбранного типа
        /// </summary>
        /// <returns></returns>
        public static async Task<List<string>> GetResourceListAsync(ResourceType type)
        {
            if (type == ResourceType.Tags)
                throw new ArgumentException("Ключевые слова не хранятся в виде файлов библиотеки");

            List<string> list = new List<string>();
            StorageFolder dir = await Root.GetFolderAsync(type.GetTableName().FirstLetterToUpperCase());
            foreach (var file in await dir.GetFilesAsync())
                list.Add(file.Name);
            return list;
        }

        /// <summary>
        /// Удалить ресурс
        /// </summary>
        /// <param name="type">Тип ресурса</param>
        /// <param name="name">Название файла с расширением</param>
        /// <returns></returns>
        public static async Task DeleteResourceAsync(ResourceType type, string name)
        {
            try
            {
                StorageFile file = await GetResourceAsync(type, name);
                await file.DeleteAsync();
            }
            catch { }
        }
        #endregion
    }
}
