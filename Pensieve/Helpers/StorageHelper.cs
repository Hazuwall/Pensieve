using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Pensieve
{
    public static class StorageHelper
    {
        /// <summary>
        /// Скопировать данную папку в выбранную папку
        /// </summary>
        /// <param name="Folder">Папка</param>
        /// <param name="DestFolder">Папка, в которую необходимо копировать</param>
        /// <param name="CollisionOption">Меры, предпринимаемые при совпадении имён файлов</param>
        /// <returns></returns>
        public async static Task CopyAsync(this StorageFolder Folder, StorageFolder DestFolder, NameCollisionOption CollisionOption)
        {
            try
            {
                StorageFolder copy = await DestFolder.CreateFolderAsync(Folder.Name, CreationCollisionOption.OpenIfExists);
                foreach (var file in await Folder.GetFilesAsync())
                    await file.CopyAsync(copy, file.Name, CollisionOption);
                foreach (var dir in await Folder.GetFoldersAsync())
                    await CopyAsync(dir, copy, CollisionOption);
            }
            catch { }
        }

        /// <summary>
        /// Скопировать файл во временную папку и получить на него ссылку
        /// </summary>
        /// <param name="File"></param>
        /// <param name="Name">Новое название файла</param>
        /// <returns></returns>
        public static async Task<StorageFile> GetTemporaryCopyAsync(StorageFile File, string Name)
        {
            return await File.CopyAsync(ApplicationData.Current.TemporaryFolder, Name, NameCollisionOption.GenerateUniqueName);
        }

        /// <summary>
        /// Скопировать файл во временную папку и получить на него ссылку
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public static async Task<StorageFile> GetTemporaryCopyAsync(StorageFile File)
        {
            return await File.CopyAsync(ApplicationData.Current.TemporaryFolder, File.Name, NameCollisionOption.GenerateUniqueName);
        }

        /// <summary>
        /// Получить папку из локального хранилища. Возвращает Null, если папка не существует
        /// </summary>
        /// <param name="Path">Путь к папке в локальном хранилище. Например "", "dir", "dir\subdir"</param>
        /// <returns>Папка, расположенная по пути, или Null при отсутствии</returns>
        public static async Task<StorageFolder> TryGetLocalFolderAsync(string Path)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\" + Path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получить папку по абсолютному пути. Возвращает null в случае неудачи
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<StorageFolder> TryGetFolderFromPathAsync(string path)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Существует ли файл в локальном хранилище
        /// </summary>
        /// <param name="Path">Путь к файлу в локальном хранилище. Например "file.ext", "dir\file.ext"</param>
        /// <returns></returns>
        public static async Task<bool> IsFileExists(string Path)
        {
            try
            {
                await StorageFile.GetFileFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\" + Path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Существует ли папка по данному пути
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static async Task<bool> IsFolderExistsAsync(string Path)
        {
            try
            {
                await StorageFolder.GetFolderFromPathAsync(Path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Очистить папку Temp приложения
        /// </summary>
        /// <returns></returns>
        public static async Task CleanTempFolderAsync()
        {
            foreach (var item in await ApplicationData.Current.TemporaryFolder.GetFilesAsync())
                try
                {
                    await item.DeleteAsync();
                }
                catch { }
        }

        /// <summary>
        /// Получить или создать папку в локальном хранилище
        /// </summary>
        /// <param name="path">Путь к папке. Например "root/dir", "root\dir\subdir"</param>
        /// <returns></returns>
        public static async Task<StorageFolder> CreateLocalFolderAsync(string path)
        {
            string[] dirNames = path.Split(new char[]{'\\', '/'},StringSplitOptions.RemoveEmptyEntries);
            StorageFolder current = ApplicationData.Current.LocalFolder;
            for (int i = 0; i < dirNames.Length; i++)
                current = await current.CreateFolderAsync(dirNames[i], CreationCollisionOption.OpenIfExists);
            return current;
        }

        /// <summary>
        /// Открыть или создать папку по абсолютному пути
        /// </summary>
        /// <param name="root">Корневая папка</param>
        /// <param name="folderPath">Относительный путь папки относительно корневой</param>
        /// <returns></returns>
        public static async Task<StorageFolder> OpenOrCreateFolderFromPathAsync(StorageFolder root, string folderPath)
        {
            bool folderExists = true;
            StorageFolder folder = null;
            try
            {
                folder = await StorageFolder.GetFolderFromPathAsync(Path.Combine(root.Path, folderPath));
            }
            catch (FileNotFoundException)
            {
                folderExists = false;
            }
            if (!folderExists)
            {
                string[] dirNames = folderPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                folder = root;
                for (int i = 0; i < dirNames.Length; i++)
                    folder = await folder.CreateFolderAsync(dirNames[i], CreationCollisionOption.OpenIfExists);
            }
            return folder;
        }

        /// <summary>
        /// Создать новый файл в локальном хранилище с заменой при совпадении имён
        /// </summary>
        /// <param name="pathToFile">Путь к папке нового файла. Например "root/dir", "root\dir\subdir"</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
        public static async Task<StorageFile> CreateLocalFileAsync(string pathToFile,string fileName)
        {
            return await (await CreateLocalFolderAsync(pathToFile)).CreateFileAsync(fileName,CreationCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// Получить источник изображения данного файла
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public static async Task<ImageSource> GetImageSourceAsync(this StorageFile File)
        {
            BitmapImage bitmap = new BitmapImage();
            using (var stream = await File.OpenAsync(FileAccessMode.Read))
                bitmap.SetSource(stream);
            return bitmap;
        }

        /// <summary>
        /// Получить источник превью-изображение данного файла
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public static async Task<ImageSource> GetThumbnailImageSourceAsync(this StorageFile File)
        {
            BitmapImage bitmap = new BitmapImage();
            using (var stream = await File.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView))
                bitmap.SetSource(stream);
            return bitmap;
        }

        #region Заполнение Zip-архивов

        /// <summary>
        /// Записать файл в поток архива
        /// </summary>
        /// <param name="stream">Поток архива</param>
        /// <param name="folder">Файл для помещения архив</param>
        /// <param name="pathToFolder">Относительный путь к файлу в архиве, например, "root/", "", "root/dir/subdir/"</param>
        /// <returns></returns>
        public async static Task PutFileAsync(this ZipOutputStream stream, StorageFile file, string pathToFile)
        {
            ZipEntry entry = new ZipEntry(pathToFile + file.Name);
            entry.DateTime = file.DateCreated.DateTime;
            stream.PutNextEntry(entry);
            byte[] buffer = new byte[4096];
            using (Stream fs = (await file.OpenAsync(FileAccessMode.Read)).AsStream())
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, sourceBytes);
                }
                while (sourceBytes > 0);
            }
        }

        /// <summary>
        /// Записывает папку в поток архива
        /// </summary>
        /// <param name="stream">Поток архива</param>
        /// <param name="folder">Папка для помещения архив</param>
        /// <param name="pathToFolder">Относительный путь к папке в архиве, например, "root/", "", "root/dir/subdir/"</param>
        /// <returns></returns>
        public async static Task PutFolderAsync(this ZipOutputStream stream, StorageFolder folder, string pathToFolder)
        {
            foreach (StorageFile file in await folder.GetFilesAsync())
                await PutFileAsync(stream, file, pathToFolder + folder.Name + "/");
            foreach (StorageFolder subfolder in await folder.GetFoldersAsync())
                await PutFolderAsync(stream, subfolder, pathToFolder + folder.Name + "/");
        }

        /// <summary>
        /// Создать новый архив
        /// </summary>
        /// <param name="File">Файл для архива</param>
        /// <param name="Content">Содержание архива</param>
        /// <param name="CompressionLevel">Уровень сжатия 0-9</param>
        /// <returns></returns>
        public static async Task CreateArchiveAsync(StorageFile File, List<IStorageItem> Content, int CompressionLevel)
        {
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultEncoding = System.Text.Encoding.GetEncoding("cp866");
            using (Stream resultStream = (await File.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
            {
                resultStream.SetLength(0);
                using (ZipOutputStream zipStream = new ZipOutputStream(resultStream))
                {
                    zipStream.SetLevel(CompressionLevel);
                    zipStream.IsStreamOwner = false;
                    foreach (IStorageItem item in Content) {
                        if (item.IsOfType(StorageItemTypes.File))
                            await PutFileAsync(zipStream, item as StorageFile, String.Empty);
                        else if (item.IsOfType(StorageItemTypes.Folder))
                            await PutFolderAsync(zipStream, item as StorageFolder, String.Empty);
                    }
                    zipStream.Finish();
                }
            }
        }
        #endregion

        #region Распаковка Zip-архивов

        /// <summary>
        /// Распаковать определелённые директории архива в указанную папку
        /// </summary>
        /// <param name="archiveFile">Файл архива</param>
        /// <param name="destFolder">Папка для распаковки</param>
        /// <param name="creationOption">Меры, предпринимаемые при совпадении имён файлов в конечной папке</param>
        /// <param name="dirs">Список директорий архива, которые следует распаковать, например, "root/", "root/dir/subdir"</param>
        /// <returns></returns>
        public async static Task UnzipArchiveAsync(this StorageFile archiveFile, StorageFolder destFolder, CreationCollisionOption creationOption, string[] dirs)
        {
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultEncoding = System.Text.Encoding.GetEncoding("cp866");
            using (Stream archiveFileStream = (await archiveFile.OpenAsync(FileAccessMode.Read)).AsStreamForRead())
            {
                using (ZipFile zipFile = new ZipFile(archiveFileStream))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (!entry.IsFile || (dirs != null && !Array.Exists(dirs, t => entry.Name.StartsWith(t))))
                            continue;

                        string[] path = entry.Name.Split('/');
                        StorageFolder outDir = destFolder;
                        for (int i = 0; i < path.Length - 1; i++)
                            outDir = await outDir.CreateFolderAsync(path[i], CreationCollisionOption.OpenIfExists);

                        using (var entryDataStream = zipFile.GetInputStream(entry))
                        {
                            StorageFile file = await outDir.CreateFileAsync(path[path.Length - 1], creationOption);
                            byte[] buffer = new byte[4096];
                            using (var fileStream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
                                StreamUtils.Copy(entryDataStream, fileStream, buffer);
                        }
                    }
                    zipFile.IsStreamOwner = false;
                }
            }
        }

        /// <summary>
        /// Распаковать архив в указанную папку
        /// </summary>
        /// <param name="archiveFile">Файл архива</param>
        /// <param name="destFolder">Папка для распаковки</param>
        /// <param name="creationOption">Меры, предпринимаемые при совпадении имён файлов в конечной папке</param>
        /// <returns></returns>
        public async static Task UnzipArchiveAsync(this StorageFile archiveFile, StorageFolder destFolder, CreationCollisionOption creationOption)
        {
            await UnzipArchiveAsync(archiveFile, destFolder, creationOption, null);
        }

        /// <summary>
        /// Распаковать архив в указанную папку с заменой одноимённых файлов
        /// </summary>
        /// <param name="archiveFile">Файл архива</param>
        /// <param name="destFolder">Папка для распаковки</param>
        /// <returns></returns>
        public static async Task UnzipArchiveAsync(this StorageFile archiveFile, StorageFolder destFolder)
        {
            await UnzipArchiveAsync(archiveFile, destFolder, CreationCollisionOption.ReplaceExisting, null);
        }
        #endregion
    }
}
